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
        Calibration = 1,
        OsAndCalibration = 2,
        Full = 3,
    }

    public partial class Vehicle
    {
        /// <summary>
        /// Replace the full contents of the PCM.
        /// </summary>
        public async Task<bool> Write(WriteType writeType, bool kernelRunning, bool recoveryMode, CancellationToken cancellationToken, Stream stream)
        {
            byte[] image = new byte[stream.Length];
            int bytesRead = await stream.ReadAsync(image, 0, (int)stream.Length);
            if (bytesRead != stream.Length)
            {
                // If this happens too much, we should try looping rather than reading the whole file in one shot.
                this.logger.AddUserMessage("Unable to load file.");
                return false;
            }

            if ((bytesRead != 512 * 1024) && (bytesRead != 1024 * 1024))
            {
                this.logger.AddUserMessage("This file is not a supported size.");
                return false;
            }

            try
            {
                this.device.ClearMessageQueue();

                if (!kernelRunning)
                {
                    // switch to 4x, if possible. But continue either way.
                    // if the vehicle bus switches but the device does not, the bus will need to time out to revert back to 1x, and the next steps will fail.
//                    if (!await this.VehicleSetVPW4x(VpwSpeed.FourX))
//                    {
//                        this.logger.AddUserMessage("Stopping here because we were unable to switch to 4X.");
//                        return false;
//                    }

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

//                    await toolPresentNotifier.Notify();

                    logger.AddUserMessage("Kernel uploaded to PCM succesfully.");
                }

                await this.device.SetTimeout(TimeoutScenario.Maximum);

                bool success;
                switch (writeType)
                {
                    case WriteType.Calibration:
                        success = await this.CalibrationWrite(cancellationToken, image);
                        break;

                    case WriteType.OsAndCalibration:
                        success = await this.OsAndCalibrationWrite(cancellationToken, image);
                        break;

                    case WriteType.Full:
                        success = await this.FullWrite(cancellationToken, image);
                        break;
                }

                await this.Cleanup();
                return true;
            }
            catch (Exception exception)
            {
                this.logger.AddUserMessage("Something went wrong. " + exception.Message);
                this.logger.AddUserMessage("Do not power off the PCM! Do not exit this program!");
                this.logger.AddUserMessage("Try flashing again. If errors continue, seek help online.");
                this.logger.AddUserMessage("https://pcmhacking.net/forums/viewtopic.php?f=3&t=6080");
                this.logger.AddDebugMessage(exception.ToString());
                return false;
            }
        }

        public async Task<bool> IsKernelRunning()
        {
            Message query = this.messageFactory.CreateFlashMemoryTypeQuery();
            for (int attempt = 0; attempt < 5; attempt++)
            {
                if (!await this.device.SendMessage(query))
                {
                    await Task.Delay(250);
                    continue;
                }

                Message reply = await this.device.ReceiveMessage();
                if (reply == null)
                {
                    await Task.Delay(250);
                    continue;
                }

                Response<UInt32> response = this.messageParser.ParseFlashMemoryType(reply);
                if (response.Status == ResponseStatus.Success)
                {
                    return true;
                }

                if (response.Status == ResponseStatus.Refused)
                {
                    return false;
                }

                await Task.Delay(250);
            }

            return false;
        }

        private async Task<bool> CalibrationWrite(CancellationToken cancellationToken, byte[] image)
        {
            // Which flash chip?
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

            FlashMemoryType memoryType;
            switch (chipIdResponse.Value)
            {
                case 0x12341234:
                    memoryType = FlashMemoryType.Intel512;
                    break;

                default:
                    this.logger.AddUserMessage("Unsupported flash chip ID " + chipIdResponse.Value + ". " +
                        Environment.NewLine +
                        "The flash memory in this PCM is not supported by this version of PCM Hammer." +
                        Environment.NewLine +
                        "Please look for a thread about this at pcmhacking.net, or create one if necessary." +
                        Environment.NewLine +
                        "We aim to add support for all flash chips over time.");
                    return false;
            }

            // Get CRC ranges
            IList<MemoryRange> ranges = this.GetMemoryRanges(memoryType);
            if (ranges == null)
            {
                this.logger.AddUserMessage("Unsupported flash memory format " + memoryType + ". " +
                    Environment.NewLine +
                    "The flash memory in this PCM is not supported by this version of PCM Hammer." +
                    Environment.NewLine +
                    "Please look for a thread about this at pcmhacking.net, or create one if necessary." +
                    Environment.NewLine +
                    "We aim to add support for all flash chips over time.");
                return false;
            }

            // TODO: check tags for each segment, fail if invalid, so we don't flash garbage.
            logger.AddUserMessage("Computing CRCs from local file...");
            this.GetCrcFromImage(ranges, image);

            logger.AddUserMessage("Requesting CRCs from PCM...");
            foreach (MemoryRange range in ranges)
            {
                Query<UInt32> crcQuery = new Query<uint>(
                    this.device,
                    () => this.messageFactory.CreateCrcQuery(range.Address, range.Size),
                    this.messageParser.ParseCrc,
                    this.logger,
                    cancellationToken);
                Response<UInt32> crcResponse = await crcQuery.Execute();

                if (crcResponse.Status != ResponseStatus.Success)
                {
                    this.logger.AddUserMessage("Unable to get CRC for memory range " + range.Address.ToString("X8") + " / " + range.Size.ToString("X8"));
                    return false;
                }

                range.ActualCrc = crcResponse.Value;

                this.logger.AddUserMessage(
                    string.Format(
                        "Range {0:X6}-{1:X6} - Local: {2:X8} - PCM: {3:X8} - {4}",
                        range.Address,
                        range.Address + (range.Size - 1),
                        range.DesiredCrc,
                        range.ActualCrc,
                        range.DesiredCrc == range.ActualCrc ? "Same" : "Different"));
            }
            
            return true;
        }

        private void GetCrcFromImage(IList<MemoryRange> ranges, byte[] image)
        {
            Crc crc = new Crc();
            foreach (MemoryRange range in ranges)
            {
                range.DesiredCrc = crc.GetCrc(image, range.Address, range.Size);
            }
        }

        private async Task<bool> OsAndCalibrationWrite(CancellationToken cancellationToken, byte[] image)
        {
            await Task.Delay(0);
            return true;
        }

        private async Task<bool> FullWrite(CancellationToken cancellationToken, byte[] image)
        {
            await Task.Delay(0);
            return true;
        }

        public IList<MemoryRange> GetMemoryRanges(FlashMemoryType flashMemoryType)
        {
            switch (flashMemoryType)
            {
                case FlashMemoryType.Intel512:
                    return new MemoryRange[]
                    {
                        new MemoryRange(0,      0x1000),
                        new MemoryRange(0x1000, 0x1000),
                        new MemoryRange(0x2000, 0x1000),
                    };

                default:
                    return null;
            }
        }
    }
}
