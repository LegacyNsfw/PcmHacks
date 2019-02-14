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
        None = 0,
        Compare,
        TestWrite,
        Calibration,
        Parameters,
        OsAndCalibration,
        Full,
    }

    public class CKernelWriter
    {
        private readonly Vehicle vehicle;
        private readonly Protocol protocol;
        private readonly ILogger logger;

        public CKernelWriter(Vehicle vehicle, Protocol protocol, ILogger logger)
        {
            this.vehicle = vehicle;
            this.protocol = protocol;
            this.logger = logger;
        }

        /// <summary>
        /// Write changes to the PCM's flash memory, or just test writing (Without 
        /// making changes) to evaluate the connection quality.
        /// </summary>
        public async Task<bool> Write(
            byte[] image,
            WriteType writeType, 
            UInt32 kernelVersion, 
            FileValidator validator,
            bool needToCheckOperatingSystem,
            CancellationToken cancellationToken)
        {
            bool success = false;

            try
            {
                await    this.vehicle.SendToolPresentNotification();
                this.vehicle.ClearDeviceMessageQueue();

                // TODO: install newer version if available.
                if (kernelVersion == 0)
                {
                    // switch to 4x, if possible. But continue either way.
                    // if the vehicle bus switches but the device does not, the bus will need to time out to revert back to 1x, and the next steps will fail.
                    if (!await this.vehicle.VehicleSetVPW4x(VpwSpeed.FourX))
                    {
                        this.logger.AddUserMessage("Stopping here because we were unable to switch to 4X.");
                        return false;
                    }

                    Response<byte[]> response = await this.vehicle.LoadKernelFromFile("write-kernel.bin");
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
                    if (!await this.vehicle.PCMExecute(response.Value, 0xFF8000, cancellationToken))
                    {
                        logger.AddUserMessage("Failed to upload kernel to PCM");

                        return false;
                    }

                    logger.AddUserMessage("Kernel uploaded to PCM succesfully.");
                }

                if (needToCheckOperatingSystem)
                {
                    if (!await this.vehicle.IsSameOperatingSystemAccordingToKernel(validator, cancellationToken))
                    {
                        this.logger.AddUserMessage("Flashing this file could render your PCM unusable.");
                        return false;
                    }
                }

                success = await this.Write(cancellationToken, image, writeType);

                // We only do cleanup after a successful write.
                // If the kernel remains running, the user can try to flash again without rebooting and reloading.
                // TODO: kernel version should be stored at a fixed location in the bin file.
                // TODO: app should check kernel version (not just "is present") and reload only if version is lower than version in kernel file.
                if (success)
                {
                    await this.vehicle.Cleanup();
                }

                return success;
            }
            catch (Exception exception)
            {
                if (!success)
                {
                    this.logger.AddUserMessage("Something went wrong. " + exception.Message);
                    this.logger.AddUserMessage("Do not power off the PCM! Do not exit this program!");
                    this.logger.AddUserMessage("Try flashing again. If errors continue, seek help online.");
                    this.logger.AddUserMessage("https://pcmhacking.net/forums/viewtopic.php?f=3&t=6080");
                    this.logger.AddUserMessage(string.Empty);
                    this.logger.AddUserMessage(exception.ToString());
                }

                return success;
            }
        }

        /// <summary>
        /// Write the calibration blocks.
        /// </summary>
        private async Task<bool> Write(CancellationToken cancellationToken, byte[] image, WriteType writeType)
        {
            await this.vehicle.SendToolPresentNotification();

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

                case WriteType.Parameters:
                    relevantBlocks = BlockType.Parameter;
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
            await this.vehicle.SendToolPresentNotification();
            await this.vehicle.SetDeviceTimeout(TimeoutScenario.ReadProperty);
            Query<UInt32> chipIdQuery = this.vehicle.CreateQuery<UInt32>(
                this.protocol.CreateFlashMemoryTypeQuery,
                this.protocol.ParseFlashMemoryType,
                cancellationToken);
            Response<UInt32> chipIdResponse = await chipIdQuery.Execute();

            if (chipIdResponse.Status != ResponseStatus.Success)
            {
                logger.AddUserMessage("Unable to determine which flash chip is in this PCM");
                return false;
            }

            logger.AddUserMessage("Flash memory type: " + chipIdResponse.Value.ToString("X8"));
            
            // known chips
            // http://ftp1.digi.com/support/documentation/jtag_v410_flashes.pdf
            string Amd   = "Amd";               // 0001
            string Intel = "Intel";             // 0089
            string I4471 = "28F400B5-B 512Kb";  // 4471
            string A2258 = "Am29F800B 1Mbyte";  // 2258
            string I889D = "28F800B5-B 1Mbyte"; // 889D
            
            logger.AddUserMessage("Flash memory ID: " + chipIdResponse.Value.ToString("X8"));
            if ((chipIdResponse.Value >> 16)    == 0x0001) logger.AddUserMessage("Flash memory manufactuer: " + Amd);
            if ((chipIdResponse.Value >> 16)    == 0x0089) logger.AddUserMessage("Flash memory manufactuer: " + Intel);
            if ((chipIdResponse.Value & 0xFFFF) == 0x4471) logger.AddUserMessage("Flash memory type: " + I4471);
            if ((chipIdResponse.Value & 0xFFFF) == 0x2258) logger.AddUserMessage("Flash memory type: " + A2258);
            if ((chipIdResponse.Value & 0xFFFF) == 0x889D) logger.AddUserMessage("Flash memory type: " + I889D);

            await this.vehicle.SendToolPresentNotification();
            IList<MemoryRange> ranges = FlashChips.GetMemoryRanges(chipIdResponse.Value, this.logger);
            if (ranges == null)
            {
                return false;
            }
            
            CKernelVerifier verifier = new CKernelVerifier(
                image, 
                ranges, 
                this.vehicle, 
                this.protocol, 
                this.logger);

            if (await verifier.CompareRanges(
                ranges, 
                image, 
                relevantBlocks, 
                cancellationToken))
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
            await this.vehicle.SetDeviceTimeout(TimeoutScenario.Maximum);
            foreach (MemoryRange range in ranges)
            {
                // We'll send a tool-present message during the erase request.
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

                    Query<byte> eraseRequest = this.vehicle.CreateQuery<byte>(
                         () => this.protocol.CreateFlashEraseBlockRequest(range.Address),
                         this.protocol.ParseFlashEraseBlock,
                         cancellationToken);

                    eraseRequest.MaxTimeouts = 5; // Reduce this when we know how many are likely to be needed.
                    Response<byte> eraseResponse = await eraseRequest.Execute();

                    if (eraseResponse.Status != ResponseStatus.Success) 
                    {
                        this.logger.AddUserMessage("Unable to erase flash memory: " + eraseResponse.Status.ToString());
                        this.RequestDebugLogs(cancellationToken);
                        return false;
                    }

                    if (eraseResponse.Value != 0x80)
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

                await this.vehicle.SendToolPresentNotification();

                await WriteMemoryRange(
                    range, 
                    image, 
                    writeType == WriteType.TestWrite, 
                    cancellationToken);
            }

            bool match = await verifier.CompareRanges(ranges, image, relevantBlocks, cancellationToken);

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
                this.logger.AddUserMessage("This PCM is probably not usable in its current state.");
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
        /// Copy a single memory range to the PCM.
        /// </summary>
        private async Task<bool> WriteMemoryRange(
            MemoryRange range, 
            byte[] image, 
            bool justTestWrite, 
            CancellationToken cancellationToken)
        {
            int devicePayloadSize = vehicle.DeviceMaxSendSize - 12; // Headers use 10 bytes, sum uses 2 bytes.
            for (int index = 0; index < range.Size; index += devicePayloadSize)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                await this.vehicle.SendToolPresentNotification();

                int startAddress = (int)(range.Address + index);
                int thisPayloadSize = Math.Min(devicePayloadSize, (int)range.Size - index);
                                                
                Message payloadMessage = protocol.CreateBlockMessage(
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

                await this.vehicle.WritePayload(payloadMessage, cancellationToken);

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
    }
}
