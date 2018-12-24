using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// How much of the PCM to erase and rewrite.
    /// </summary>
    public enum WriteType
    {
        Invalid = 0,
        Compare,
        TestWrite,
        Calibration,
        OsAndCalibration,
        Full,
    }

    public partial class Vehicle
    {
        /// <summary>
        /// Write changes to the PCM's flash memory, or just test writing (Without 
        /// making changes) to evaluate the connection quality.
        /// </summary>
        public async Task<bool> Write(WriteType writeType, UInt32 kernelVersion, CancellationToken cancellationToken, byte[] image)
        {
            try
            {
                ToolPresentNotifier notifier = new ToolPresentNotifier(this.logger, this.messageFactory, this.device);
                this.device.ClearMessageQueue();

                // TODO: install newer version if available.
                if (kernelVersion == 0)
                {
                    // switch to 4x, if possible. But continue either way.
                    // if the vehicle bus switches but the device does not, the bus will need to time out to revert back to 1x, and the next steps will fail.
                    if (!await this.VehicleSetVPW4x(VpwSpeed.FourX))
                    {
                        this.logger.AddUserMessage("Stopping here because we were unable to switch to 4X.");
                        return false;
                    }

                    Response<byte[]> response = await LoadKernelFromFile("write-kernel.bin");
                    if (response.Status != ResponseStatus.Success)
                    {
                        logger.AddUserMessage("Failed to load kernel from file.");
                        return false;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    // TODO: instead of this hard-coded address, get the base address from the PcmInfo object.
                    if (!await PCMExecute(response.Value, 0xFF8000, cancellationToken))
                    {
                        logger.AddUserMessage("Failed to upload kernel to PCM");

                        return false;
                    }

                    logger.AddUserMessage("Kernel uploaded to PCM succesfully.");
                }

                bool success = await Write(cancellationToken, image, writeType, notifier);

                // We only do cleanup after a successful write.
                // If the kernel remains running, the user can try to flash again without rebooting and reloading.
                // TODO: kernel should send tool-present messages to keep itself in control.
                // TODO: kernel version should be stored at a fixed location in the bin file.
                // TODO: app should check kernel version (not just "is present") and reload only if version is lower than version in kernel file.
                if (success)
                {
                    await this.Cleanup();
                }

                return success;
            }
            catch (Exception exception)
            {
                this.logger.AddUserMessage("Something went wrong. " + exception.Message);
                this.logger.AddUserMessage("Do not power off the PCM! Do not exit this program!");
                this.logger.AddUserMessage("Try flashing again. If errors continue, seek help online.");
                this.logger.AddUserMessage("https://pcmhacking.net/forums/viewtopic.php?f=3&t=6080");
                this.logger.AddUserMessage(string.Empty);
                this.logger.AddUserMessage(exception.ToString());
                return false;
            }
        }

        /// <summary>
        /// Write the calibration blocks.
        /// </summary>
        private async Task<bool> Write(CancellationToken cancellationToken, byte[] image, WriteType writeType, ToolPresentNotifier notifier)
        {
            BlockType relevantBlocks;
            switch (writeType)
            {
                case WriteType.Compare:
                    relevantBlocks = BlockType.All;
                    break;

                case WriteType.TestWrite:
                    relevantBlocks = BlockType.Calibration;
                    break;

                case WriteType.Calibration:
                    relevantBlocks = BlockType.Calibration;
                    break;

                case WriteType.OsAndCalibration:
                    relevantBlocks = BlockType.Calibration | BlockType.OperatingSystem;
                    break;

                case WriteType.Full:
                    relevantBlocks = BlockType.All;
                    break;

                default:
                    throw new InvalidDataException("Unsuppported operation type: " + writeType.ToString());
            }

            // Which flash chip?
            await this.device.SetTimeout(TimeoutScenario.ReadProperty);
            Query<UInt32> chipIdQuery = new Query<uint>(
                this.device,
                this.messageFactory.CreateFlashMemoryTypeQuery,
                this.messageParser.ParseFlashMemoryType,
                this.logger,
                cancellationToken);
            Response<UInt32> chipIdResponse = await chipIdQuery.Execute();

            if (chipIdResponse.Status != ResponseStatus.Success)
            {
                logger.AddUserMessage("Unable to determine which flash chip is in this PCM");
                return false;
            }

            logger.AddUserMessage("Flash memory type: " + chipIdResponse.Value.ToString("X8"));

            IList<MemoryRange> ranges = this.GetMemoryRanges(chipIdResponse.Value);
            if (ranges == null)
            {
                return false;
            }

            logger.AddUserMessage("Computing CRCs from local file...");
            this.GetCrcFromImage(ranges, image);

            if (await this.CompareRanges(
                ranges, 
                image, 
                relevantBlocks, 
                cancellationToken, notifier))
            {
                // Don't stop here if the user just wants to test their cable.
                if (writeType != WriteType.TestWrite)
                {
                    this.logger.AddUserMessage("All ranges are identical.");
                    return true;
                }
            }

            // Stop now if the user only requested a comparison.
            if (writeType == WriteType.Compare)
            {
                this.logger.AddUserMessage("Note that mismatched Parameter blocks are to be expected.");
                this.logger.AddUserMessage("Parameter data can change every time the PCM is used.");
                return true;
            }

            // Erase and rewrite the required memory ranges.
            await this.device.SetTimeout(TimeoutScenario.Maximum);
            foreach (MemoryRange range in ranges)
            {
                if ((range.ActualCrc == range.DesiredCrc) && (writeType != WriteType.TestWrite))
                {
                    continue;
                }

                if ((range.Type & relevantBlocks) == 0)
                {
                    continue;
                }

                this.logger.AddUserMessage(
                    string.Format(
                        "Processing range {0:X6}-{1:X6}",
                        range.Address,
                        range.Address + (range.Size - 1)));

                if (writeType != WriteType.TestWrite)
                {
                    this.logger.AddUserMessage("Erasing");

                    Query<byte> eraseRequest = new Query<byte>(
                         this.device,
                         this.messageFactory.CreateFlashEraseCalibrationRequest,
                         this.messageParser.ParseFlashErase,
                         this.logger,
                         cancellationToken,
                         notifier);

                    eraseRequest.MaxTimeouts = 50; // Reduce this when we know how many are likely to be needed.
                    Response<byte> eraseResponse = await eraseRequest.Execute();

                    if (eraseResponse.Status != ResponseStatus.Success)
                    {
                        this.logger.AddUserMessage("Unable to erase flash memory. Code: " + eraseResponse.Value.ToString("X2"));
                        this.RequestDebugLogs(cancellationToken);
                        return false;
                    }
                }

                if (writeType == WriteType.TestWrite)
                {
                    this.logger.AddUserMessage("Testing...");
                }
                else
                {
                    this.logger.AddUserMessage("Writing...");
                }

                await WriteMemoryRange(
                    range, 
                    image, 
                    writeType == WriteType.TestWrite, 
                    cancellationToken);
            }

            bool match = await this.CompareRanges(ranges, image, relevantBlocks, cancellationToken, notifier);

            if (writeType == WriteType.TestWrite)
            {
                // TODO: the app should know if any errors were encountered. The user shouldn't need to check.
                this.logger.AddUserMessage("Test complete. Were any errors logged above?");
                return true;
            }

            if (match)
            {
                this.logger.AddUserMessage("Flash successful!");
                return true;
            }

            this.logger.AddUserMessage("===============================================");
            this.logger.AddUserMessage("THE CHANGES WERE -NOT- WRITTEN SUCCESSFULLY");
            this.logger.AddUserMessage("===============================================");

            if (cancellationToken.IsCancellationRequested)
            {
                this.logger.AddUserMessage("");
                this.logger.AddUserMessage("The operation was cancelled.");
                this.logger.AddUserMessage("");
            }
            else
            {
                this.logger.AddUserMessage("Don't panic. Also, don't try to drive this car.");
                this.logger.AddUserMessage("Please try flashing again. Preferably now.");
                this.logger.AddUserMessage("In most cases, the second try will succeed.");
                this.logger.AddUserMessage("");
                this.logger.AddUserMessage("If this happens three times in a row, please");
                this.logger.AddUserMessage("start a new thread at pcmhacking.net, and");
                this.logger.AddUserMessage("include the contents of the debug tab.");
                this.logger.AddUserMessage("");
                this.RequestDebugLogs(cancellationToken);
            }

            return false;
        }

        /// <summary>
        /// Get the CRC for each address range in the file that the user wants to flash.
        /// </summary>
        private void GetCrcFromImage(IList<MemoryRange> ranges, byte[] image)
        {
            Crc crc = new Crc();
            foreach (MemoryRange range in ranges)
            {
                range.DesiredCrc = crc.GetCrc(image, range.Address, range.Size);
            }
        }

        /// <summary>
        /// Compare CRCs from the file to CRCs from the PCM.
        /// </summary>
        private async Task<bool> CompareRanges(IList<MemoryRange> ranges, byte[] image, BlockType blockTypes, CancellationToken cancellationToken, ToolPresentNotifier notifier)
        {
            logger.AddUserMessage("Requesting CRCs from PCM...");

            await this.device.SetTimeout(TimeoutScenario.ReadCrc);
            bool successForAllRanges = true;
            foreach (MemoryRange range in ranges)
            {
                if ((range.Type & blockTypes) == 0)
                {
                    this.logger.AddUserMessage(
                    string.Format(
                        "Range {0:X6}-{1:X6} - Not needed for this operation.",
                        range.Address,
                        range.Address + (range.Size - 1)));
                    continue;
                }

                this.device.ClearMessageQueue();
                bool success = false;
                UInt32 crc = 0;
                
                // You might think that a shorter retry delay would speed things up,
                // but 1500ms delay gets CRC results in about 3.5 seconds.
                // A 1000ms delay resulted in 4+ second CRC responses, and a 750ms
                // delay resulted in 5 second CRC responses. The PCM needs to spend
                // its time caculating CRCs rather than responding to messages.
                int retryDelay = 1500;
                Message query = this.messageFactory.CreateCrcQuery(range.Address, range.Size);
                for (int attempts = 0; attempts < 10; attempts++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (!await this.device.SendMessage(query))
                    {
                        // This delay is fast because we're waiting for the bus to be available,
                        // rather than waiting for the PCM's CPU to finish computing the CRC as 
                        // with the other two delays below.
                        await Task.Delay(100);
                        continue;
                    }

                    Message response = await this.device.ReceiveMessage();
                    if (response == null)
                    {
                        await Task.Delay(retryDelay);
                        continue;
                    }

                    Response<UInt32> crcResponse = this.messageParser.ParseCrc(response, range.Address, range.Size);
                    if (crcResponse.Status != ResponseStatus.Success)
                    {
                        await Task.Delay(retryDelay);
                        continue;
                    }

                    success = true;
                    crc = crcResponse.Value;
                    break;
                }

                this.device.ClearMessageQueue();

                if (!success)
                {
                    this.logger.AddUserMessage("Unable to get CRC for memory range " + range.Address.ToString("X8") + " / " + range.Size.ToString("X8"));
                    successForAllRanges = false;
                    continue;
                }

                range.ActualCrc = crc;

                this.logger.AddUserMessage(
                    string.Format(
                        "Range {0:X6}-{1:X6} - Local: {2:X8} - PCM: {3:X8} - ({4}) - {5}",
                        range.Address,
                        range.Address + (range.Size - 1),
                        range.DesiredCrc,
                        range.ActualCrc,
                        range.Type,
                        range.DesiredCrc == range.ActualCrc ? "Same" : "Different"));
            }

            foreach (MemoryRange range in ranges)
            {
                if ((range.Type & blockTypes) == 0)
                {
                    continue;
                }

                if (range.ActualCrc != range.DesiredCrc)
                {
                    return false;
                }
            }

            this.device.ClearMessageQueue();

            return successForAllRanges;
        }

        /// <summary>
        /// Copy a single memory range to the PCM.
        /// </summary>
        private async Task<bool> WriteMemoryRange(MemoryRange range, byte[] image, bool justTestWrite, CancellationToken cancellationToken)
        {
            int devicePayloadSize = device.MaxSendSize - 12; // Headers use 10 bytes, sum uses 2 bytes.
            for (int index = 0; index < range.Size; index += devicePayloadSize)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                int startAddress = (int)(range.Address + index);
                int thisPayloadSize = Math.Min(devicePayloadSize, (int)range.Size - index);
                                                
                Message payloadMessage = messageFactory.CreateBlockMessage(
                    image,
                    startAddress,
                    thisPayloadSize,
                    startAddress,
                    justTestWrite ? BlockCopyType.TestWrite : BlockCopyType.Copy);

                logger.AddUserMessage(
                    string.Format(
                        "Sending payload with offset 0x{0:X4}, start address 0x{1:X6}, length 0x{2:X4}.",
                        index,
                        startAddress,
                        thisPayloadSize));

                await this.WritePayload(payloadMessage, cancellationToken);

                // Not checking the success or failure here.
                // The debug pane will show if anything goes wrong, and the CRC check at the end will alert the user.
            }

            return true;
        }

        /// <summary>
        /// Ask the user for diagnostic information, unless they cancelled.
        private void RequestDebugLogs(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                this.logger.AddUserMessage("Select the debug tab, click anywhere in the text,");
                this.logger.AddUserMessage("press Ctrl+A to select the text, and Ctrl+C to");
                this.logger.AddUserMessage("copy the text. Press Ctrl+V to paste that content");
                this.logger.AddUserMessage("content into your forum post.");
            }
        }

        /// <summary>
        /// Not yet implemented. 
        /// </summary>
        private async Task<bool> OsAndCalibrationWrite(CancellationToken cancellationToken, byte[] image)
        {
            await Task.Delay(0);
            return true;
        }

        /// <summary>
        /// Not yet implemented.
        /// </summary>
        private async Task<bool> FullWrite(CancellationToken cancellationToken, byte[] image)
        {
            await Task.Delay(0);
            return true;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chipId"></param>
        /// <returns></returns>
        public IList<MemoryRange> GetMemoryRanges(UInt32 chipId)
        {
            IList<MemoryRange> result = null;

            switch (chipId)
            {
                // This is only here as a warning to anyone adding ranges for another chip.
                // Please read the comments carefully. See case 0x00894471 for the real deal.
                case 0xFFFF4471:
                    var unused = new MemoryRange[]
                    {
                        // These numbers and descriptions are straight from the data sheet. 
                        // Notice that if you convert the hex sizes to decimal, they're all
                        // half as big as the description indicates. That's wrong. It doesn't
                        // work that way in the PCM, so this would only compare 256 kb.
                        new MemoryRange(0x30000, 0x10000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x20000, 0x10000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x10000, 0x10000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x04000, 0x0C000, BlockType.Calibration), //  96kb main block 
                        new MemoryRange(0x03000, 0x01000, BlockType.Parameter), //   8kb parameter block
                        new MemoryRange(0x02000, 0x01000, BlockType.Parameter), //   8kb parameter block
                        new MemoryRange(0x00000, 0x02000, BlockType.Boot), //  16kb boot block                        
                    };
                    return null;

                // Intel 28F400B
                case 0x00894471:
                    result = new MemoryRange[]
                    {
                        // All of these addresses and sizes are all 2x what's listed 
                        // in the data sheet, because the data sheet table assumes that
                        // "bytes" are 16 bits wide. Which means they're not bytes. But
                        // the data sheet calls them bytes.
                        new MemoryRange(0x60000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x40000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x20000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x08000, 0x18000, BlockType.Calibration), //  96kb main block 
                        new MemoryRange(0x06000, 0x02000, BlockType.Parameter), //   8kb parameter block
                        new MemoryRange(0x04000, 0x02000, BlockType.Parameter), //   8kb parameter block
                        new MemoryRange(0x00000, 0x04000, BlockType.Boot), //  16kb boot block
                    };
                    break;

                default:
                    this.logger.AddUserMessage(
                        "Unsupported flash chip ID " + chipId.ToString("X8") + ". " +
                        Environment.NewLine +
                        "The flash memory in this PCM is not supported by this version of PCM Hammer." +
                        Environment.NewLine +
                        "Please look for a thread about this at pcmhacking.net, or create one if necessary." +
                        Environment.NewLine +
                        "We do aim to support for all flash chips over time.");
                    return null;
            }

            // Sanity check the memory ranges
            UInt32 lastStart = UInt32.MaxValue;
            for(int index = 0; index < result.Count; index++)
            {
                if (index == 0)
                {
                    UInt32 top = result[index].Address + result[index].Size;
                    if ((top != 512 * 1024) && (top != 1024 * 1024))
                    {
                        throw new InvalidOperationException("Upper end of memory range must be 512k or 1024k.");
                    }
                }

                if (index == result.Count - 1)
                {
                    if (result[index].Address != 0)
                    {
                        throw new InvalidOperationException("Memory ranges must start at zero.");
                    }
                }

                if (lastStart != UInt32.MaxValue)
                {
                    if (lastStart != result[index].Address + result[index].Size)
                    {
                        throw new InvalidDataException("Top of this range must match base of range above.");
                    }
                }

                lastStart = result[index].Address;
            }

            return result;
        }
    }
}
