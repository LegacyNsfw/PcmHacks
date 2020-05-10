﻿using System;
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
        OsPlusCalibrationPlusBoot,
        Full,
    }

    public class CKernelWriter
    {
        private readonly Vehicle vehicle;
        private readonly Protocol protocol;
        private readonly WriteType writeType;
        private readonly ILogger logger;

        public CKernelWriter(Vehicle vehicle, Protocol protocol, WriteType writeType, ILogger logger)
        {
            this.vehicle = vehicle;
            this.protocol = protocol;
            this.writeType = writeType;
            this.logger = logger;
        }

        /// <summary>
        /// Write changes to the PCM's flash memory, or just test writing (Without 
        /// making changes) to evaluate the connection quality.
        /// </summary>
        public async Task<bool> Write(
            byte[] image,
            UInt32 kernelVersion, 
            FileValidator validator,
            bool needToCheckOperatingSystem,
            CancellationToken cancellationToken)
        {
            bool success = false;

            try
            {
                // Start with known state.
                await this.vehicle.ForceSendToolPresentNotification();
                this.vehicle.ClearDeviceMessageQueue();

                // TODO: install newer version if available.
                if (kernelVersion == 0)
                {
                    // Switch to 4x, if possible. But continue either way.
                    if (Configuration.Enable4xReadWrite)
                    {
                        // if the vehicle bus switches but the device does not, the bus will need to time out to revert back to 1x, and the next steps will fail.
                        if (!await this.vehicle.VehicleSetVPW4x(VpwSpeed.FourX))
                        {
                            this.logger.AddUserMessage("Stopping here because we were unable to switch to 4X.");
                            return false;
                        }
                    }
                    else
                    {
                        this.logger.AddUserMessage("4X communications disabled by configuration.");
                    }

                    Response<byte[]> response = await this.vehicle.LoadKernelFromFile("kernel.bin");
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

                // Confirm operating system match
                await this.vehicle.SendToolPresentNotification();
                await this.vehicle.SetDeviceTimeout(TimeoutScenario.ReadProperty);
                Response<UInt32> osidResponse = await this.vehicle.QueryOperatingSystemIdFromKernel(cancellationToken);
                if (needToCheckOperatingSystem && (osidResponse.Status != ResponseStatus.Success))
                {
                    // The kernel seems broken. This shouldn't happen, but if it does, halt.
                    this.logger.AddUserMessage("The kernel did not respond to operating system ID query.");
                    return false;
                }

                bool shouldHalt;
                Utility.ReportOperatingSystems(validator.GetOsidFromImage(), osidResponse.Value, this.writeType, this.logger, out shouldHalt);
                if (needToCheckOperatingSystem && shouldHalt)
                {
                    return false;
                }

                success = await this.Write(cancellationToken, image);

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
                    switch (this.writeType)
                    {
                        case WriteType.None:
                        case WriteType.Compare:
                        case WriteType.TestWrite:
                            await this.vehicle.Cleanup();
                            this.logger.AddUserMessage("Something has gone wrong. Please report this error.");
                            this.logger.AddUserMessage("Errors during comparisons or test writes indicate a");
                            this.logger.AddUserMessage("problem with the PCM, interface, cable, or app. Don't");
                            this.logger.AddUserMessage("try to do any actual writing until you are certain that");
                            this.logger.AddUserMessage("the underlying problem has been completely corrected.");
                            break;

                        default:
                            this.logger.AddUserMessage("Something went wrong. " + exception.Message);
                            this.logger.AddUserMessage("Do not power off the PCM! Do not exit this program!");
                            this.logger.AddUserMessage("Try flashing again. If errors continue, seek help online.");
                            break;
                    }

                    this.logger.AddUserMessage("https://pcmhacking.net/forums/viewtopic.php?f=42&t=6080");
                    this.logger.AddUserMessage(string.Empty);
                    this.logger.AddUserMessage(exception.ToString());
                }

                return success;
            }
        }

        /// <summary>
        /// Write the calibration blocks.
        /// </summary>
        private async Task<bool> Write(CancellationToken cancellationToken, byte[] image)
        {
            await this.vehicle.SendToolPresentNotification();

            BlockType relevantBlocks;
            switch (this.writeType)
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

                case WriteType.OsPlusCalibrationPlusBoot:
                    // Overwriting parameter blocks would break the EBCM pairing, 
                    // which is not what most users want. They just want a new OS 
                    // and the calibration to go along with it.
                    //
                    // The cast seems redundant, but C# converts the enum values 
                    // to ints when it does arithmetic.
                    relevantBlocks = (BlockType)(BlockType.All - BlockType.Parameter);
                    break;

                case WriteType.Full:
                    relevantBlocks = BlockType.All;
                    break;

                default:
                    throw new InvalidDataException("Unsuppported operation type: " + this.writeType.ToString());
            }

            // Which flash chip?
            await this.vehicle.SendToolPresentNotification();
            UInt32 chipId = await this.vehicle.QueryFlashChipId(cancellationToken);
            FlashChip flashChip = FlashChip.Create(chipId, this.logger);
            logger.AddUserMessage("Flash chip: " + flashChip.ToString());

            if (flashChip.Size != image.Length)
            {
                this.logger.AddUserMessage(string.Format("File size {0:n0} does not match Flash Chip size {1:n0}!", image.Length, flashChip.Size));
                await this.vehicle.Cleanup();
                return false;
            }

            CKernelVerifier verifier = new CKernelVerifier(
                image,
                flashChip.MemoryRanges,
                this.vehicle,
                this.protocol,
                this.logger);

            int messageRetryCount = 0;
            await this.vehicle.SendToolPresentNotification();
            for (int attempt = 1; attempt <= 5; attempt++)
            {
                if (await verifier.CompareRanges(
                    image,
                    relevantBlocks,
                    cancellationToken))
                {
                    // Don't stop here if the user just wants to test their cable.
                    if (this.writeType == WriteType.TestWrite)
                    {
                        if (attempt == 1)
                        {
                            this.logger.AddUserMessage("Beginning test.");
                        }
                    }
                    else
                    {
                        this.logger.AddUserMessage("All relevant ranges are identical.");
                        if (attempt > 1)
                        {
                            Utility.ReportRetryCount("Write", messageRetryCount, flashChip.Size, this.logger);
                            this.logger.AddUserMessage("Flash successful!");
                        }
                        return true;
                    }
                }

                // For test writes, report results after the first iteration, then we're done.
                if ((this.writeType == WriteType.TestWrite) && (attempt > 1))
                {
                    logger.AddUserMessage("Test write complete.");
                    Utility.ReportRetryCount("Write", messageRetryCount, flashChip.Size, this.logger);
                    return true;
                }

                // Stop now if the user only requested a comparison.
                if (this.writeType == WriteType.Compare)
                {
                    this.logger.AddUserMessage("Note that mismatched Parameter blocks are to be expected.");
                    this.logger.AddUserMessage("Parameter data can change every time the PCM is used.");
                    return true;
                }

                // Erase and rewrite the required memory ranges.
                DateTime startTime = DateTime.Now;
                UInt32 totalSize = this.GetTotalSize(flashChip, relevantBlocks);
                UInt32 bytesRemaining = totalSize;
                foreach (MemoryRange range in flashChip.MemoryRanges)
                {
                    // We'll send a tool-present message during the erase request.
                    if (!this.ShouldProcess(range, relevantBlocks))
                    {
                        continue;
                    }

                    this.logger.AddUserMessage(
                        string.Format(
                            "Processing range {0:X6}-{1:X6}",
                            range.Address,
                            range.Address + (range.Size - 1)));

                    if (this.writeType == WriteType.TestWrite)
                    {
                        this.logger.AddUserMessage("Pretending to erase.");
                    }
                    else
                    {
                        if (!await this.EraseMemoryRange(range, cancellationToken))
                        {
                            return false;
                        }
                    }

                    if (this.writeType == WriteType.TestWrite)
                    {
                        this.logger.AddUserMessage("Pretending to write...");
                    }
                    else
                    {
                        this.logger.AddUserMessage("Writing...");
                    }

                    this.logger.AddUserMessage("Address\t% Done\tTime Remaining");

                    Response<bool> writeResponse = await WriteMemoryRange(
                        range,
                        image,
                        this.writeType == WriteType.TestWrite,
                        startTime,
                        totalSize,
                        bytesRemaining,
                        cancellationToken);

                    if (writeResponse.RetryCount > 0)
                    {
                        this.logger.AddUserMessage("Retry count for this block: " + writeResponse.RetryCount);
                        messageRetryCount += writeResponse.RetryCount;
                    }

                    if (writeResponse.Value)
                    {
                        bytesRemaining -= range.Size;
                    }
                }
            }

            // During a test write, we will return from the middle of the loop above.
            // So if we made it here, a real write has failed.
            this.logger.AddUserMessage("===============================================");
            this.logger.AddUserMessage("THE CHANGES WERE -NOT- WRITTEN SUCCESSFULLY");
            this.logger.AddUserMessage("===============================================");

            if (this.writeType == WriteType.Calibration)
            {
                this.logger.AddUserMessage("Erasing Calibration to force recovery mode.");
                this.logger.AddUserMessage("");

                foreach(MemoryRange range in flashChip.MemoryRanges)
                {
                    if (range.Type == BlockType.Calibration)
                    {
                        await this.EraseMemoryRange(range, cancellationToken);
                    }
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                this.logger.AddUserMessage("");
                this.logger.AddUserMessage("The operation was cancelled.");
                this.logger.AddUserMessage("This PCM is probably not usable in its current state.");
                this.logger.AddUserMessage("");
            }
            else
            {
                this.logger.AddUserMessage("This may indicate a hardware problem on the PCM.");
                this.logger.AddUserMessage("We tried, and re-tried, and it still didn't work.");
                this.logger.AddUserMessage("");
                this.logger.AddUserMessage("Please start a new thread at pcmhacking.net, and");
                this.logger.AddUserMessage("include the contents of the debug tab.");
                this.RequestDebugLogs(cancellationToken);
            }

            return false;
        }

        private UInt32 GetTotalSize(FlashChip chip, BlockType relevantBlocks)
        {
            UInt32 result = 0;
            foreach(MemoryRange range in chip.MemoryRanges)
            {
                if (this.ShouldProcess(range, relevantBlocks))
                {
                    result += range.Size;
                }
            }

            return result;
        }

        private bool ShouldProcess(MemoryRange range, BlockType relevantBlocks)
        {
            if ((range.ActualCrc == range.DesiredCrc) && (this.writeType != WriteType.TestWrite))
            {
                return false;
            }

            // Skip irrelevant blocks.
            if ((range.Type & relevantBlocks) == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Erase a block in the flash memory.
        /// </summary>
        private async Task<bool> EraseMemoryRange(MemoryRange range, CancellationToken cancellationToken)
        {
            this.logger.AddUserMessage("Erasing.");

            await this.vehicle.SetDeviceTimeout(TimeoutScenario.EraseMemoryBlock);
            Query<byte> eraseRequest = this.vehicle.CreateQuery<byte>(
                 () => this.protocol.CreateFlashEraseBlockRequest(range.Address),
                 this.protocol.ParseFlashEraseBlock,
                 cancellationToken);

            eraseRequest.MaxTimeouts = 3;
            Response<byte> eraseResponse = await eraseRequest.Execute();

            if (eraseResponse.Status != ResponseStatus.Success)
            {
                this.logger.AddUserMessage("Unable to erase flash memory: " + eraseResponse.Status.ToString());
                this.RequestDebugLogs(cancellationToken);
                return false;
            }

            if (eraseResponse.Value != 0x00)
            {
                this.logger.AddUserMessage("Unable to erase flash memory. Code: " + eraseResponse.Value.ToString("X2"));
                this.RequestDebugLogs(cancellationToken);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Copy a single memory range to the PCM.
        /// </summary>
        private async Task<Response<bool>> WriteMemoryRange(
            MemoryRange range,
            byte[] image,
            bool justTestWrite,
            DateTime startTime,
            UInt32 totalSize,
            UInt32 bytesRemaining,
            CancellationToken cancellationToken)
        {
            int retryCount = 0;
            int devicePayloadSize = vehicle.DeviceMaxFlashWriteSendSize - 12; // Headers use 10 bytes, sum uses 2 bytes.
            for (int index = 0; index < range.Size; index += devicePayloadSize)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Response.Create(ResponseStatus.Cancelled, false, retryCount);
                }

                await this.vehicle.SendToolPresentNotification();

                int startAddress = (int)(range.Address + index);
                UInt32 thisPayloadSize = (UInt32) Math.Min(devicePayloadSize, (int)range.Size - index);

                logger.AddDebugMessage(
                    string.Format(
                        "Sending payload with offset 0x{0:X4}, start address 0x{1:X6}, length 0x{2:X4}.",
                        index,
                        startAddress,
                        thisPayloadSize));

                Message payloadMessage = protocol.CreateBlockMessage(
                    image,
                    startAddress,
                    (int)thisPayloadSize,
                    startAddress,
                    justTestWrite ? BlockCopyType.TestWrite : BlockCopyType.Copy);

                string timeRemaining;

                TimeSpan elapsed = DateTime.Now - startTime;
                UInt32 totalWritten = totalSize - bytesRemaining;

                // Wait 10 seconds before showing estimates.
                if (elapsed.TotalSeconds < 10)
                {
                    timeRemaining = "Measuring write speed...";
                }
                else
                {
                    UInt32 bytesPerSecond = (UInt32)(totalWritten / elapsed.TotalSeconds);

                    // Don't divide by zero.
                    if (bytesPerSecond > 0)
                    {
                        UInt32 secondsRemaining = (UInt32)(bytesRemaining / bytesPerSecond);
                        timeRemaining = TimeSpan.FromSeconds(secondsRemaining).ToString("mm\\:ss");
                    }
                    else
                    {
                        timeRemaining = "??:??";
                    }
                }

                logger.AddUserMessage(
                    string.Format(
                        "0x{0:X6}\t{1}%\t{2}",
                        startAddress,
                        totalWritten * 100 / totalSize,
                        timeRemaining));

                await this.vehicle.SetDeviceTimeout(TimeoutScenario.WriteMemoryBlock);

                // WritePayload contains a retry loop, so if it fails, we don't need to retry at this layer.
                Response<bool> response = await this.vehicle.WritePayload(payloadMessage, cancellationToken);
                if (response.Status != ResponseStatus.Success)
                {
                    return Response.Create(ResponseStatus.Error, false, response.RetryCount);
                }

                bytesRemaining -= thisPayloadSize;
                retryCount += response.RetryCount;
            }

            return Response.Create(ResponseStatus.Success, true, retryCount);
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
