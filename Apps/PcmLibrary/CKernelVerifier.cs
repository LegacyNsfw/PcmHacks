using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PcmHacking
{
    public class CKernelVerifier
    {
        private readonly byte[] image;
        private readonly IEnumerable<MemoryRange> ranges;
        private readonly Vehicle vehicle;
        private readonly Protocol protocol;
        private readonly PcmInfo pcmInfo;
        private readonly ILogger logger;

        public CKernelVerifier(
            byte[] image, 
            IEnumerable<MemoryRange> ranges, 
            Vehicle vehicle, 
            Protocol protocol, 
            PcmInfo pcmInfo,
            ILogger logger)
        {
            this.image = image;
            this.ranges = ranges;
            this.vehicle = vehicle;
            this.protocol = protocol;
            this.pcmInfo = pcmInfo;
            this.logger = logger;
        }

        /// <summary>
        /// Get the CRC for each address range in the file that the user wants to flash.
        /// </summary>
        private void GetCrcFromImage()
        {
            Crc crc = new Crc();
            foreach (MemoryRange range in this.ranges)
            {
                if (range.Address < pcmInfo.ImageSize) // P10 does not use the whole chip
                {
                    range.DesiredCrc = crc.GetCrc(this.image, range.Address, range.Size);
                }
            }
        }

        /// <summary>
        /// Compare CRCs from the file to CRCs from the PCM.
        /// </summary>
        public async Task<bool> CompareRanges(
            byte[] image, 
            BlockType blockTypes,
            CancellationToken cancellationToken)
        {
            // This only takes a fraction of a second.
            logger.AddUserMessage("Calculating CRCs from file.");
            this.GetCrcFromImage();

            // The kernel will remember (and return) the CRC value of the last block it 
            // was asked about, which leads to misleading results if you only rewrite 
            // a single block. So we send a a bogus query to reset the last-used CRC 
            // value in the kernel.
            //
            // The first time we do this, the PCM will initialize the lookup table
            // required by the CRC algorithm, which takes about 20 seconds. Warn the
            // user, because the app appears to be hung while it waits for the response
            // to this message.
            logger.AddUserMessage("Initializing CRC algorithm on PCM, this will take a minute...");

            await this.vehicle.SendToolPresentNotification();
            Query<UInt32> crcReset = this.vehicle.CreateQuery<uint>(
                () => this.protocol.CreateCrcQuery(0, 0),
                (message) => this.protocol.ParseCrc(message, 0, 0),
                cancellationToken);
            await crcReset.Execute();

            logger.AddUserMessage("Requesting CRCs from PCM.");

            await this.vehicle.SetDeviceTimeout(TimeoutScenario.ReadCrc);
            bool successForAllRanges = true;

            logger.AddUserMessage("\tRange\t\tFile CRC\t\tPCM CRC\tVerdict\tPurpose");
            foreach (MemoryRange range in this.ranges)
            {
                string formatString = "{0:X6}-{1:X6}\t{2:X8}\t{3:X8}\t{4}\t{5}";

                if (((range.Type & blockTypes) == 0) ||
                    (range.Address >= this.pcmInfo.ImageSize))
                {
                    this.logger.AddUserMessage(
                    string.Format(
                        formatString,
                        range.Address,
                        range.Address + (range.Size - 1),
                        "not needed",
                        "not needed",
                        "n/a",
                        range.Type));
                    continue;
                }

                await this.vehicle.SendToolPresentNotification();
                this.vehicle.ClearDeviceMessageQueue();
                bool success = false;
                UInt32 crc = 0;
                
                // Each poll of the pcm causes it to CRC 16kb of segment data.
                // When the segment sum is available it is returned.
                int retryDelay = 50;
                int maxAttempts = 50; // Logged highs of 38 on 1m P12, the rest are a good deal lower.
                Message query = this.protocol.CreateCrcQuery(range.Address, range.Size);
                for (int segment = 0; segment < maxAttempts; segment++)
                {
                    logger.StatusUpdateActivity($"Processing CRC for range {range.Address:X6}-{range.Address + (range.Size - 1):X6}");
                    logger.StatusUpdateProgressBar((double)segment / maxAttempts, true);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    await this.vehicle.SendToolPresentNotification();
                    if (!await this.vehicle.SendMessage(query))
                    {
                        continue;
                    }

                    Message response = await this.vehicle.ReceiveMessage();
                    if (response == null)
                    {
                        await Task.Delay(retryDelay);
                        continue;
                    }

                    Response<UInt32> crcResponse = this.protocol.ParseCrc(response, range.Address, range.Size);
                    if (crcResponse.Status != ResponseStatus.Success)
                    {
                        await Task.Delay(retryDelay);
                        continue;
                    }

                    success = true;
                    crc = crcResponse.Value;
                    break;
                }

                logger.StatusUpdateProgressBar(0, false);
                this.vehicle.ClearDeviceMessageQueue();

                if (!success)
                {
                    this.logger.AddUserMessage("Unable to get CRC for memory range " + range.Address.ToString("X8") + " / " + range.Size.ToString("X8"));
                    successForAllRanges = false;
                    continue;
                }

                range.ActualCrc = crc;

                this.logger.AddUserMessage(
                    string.Format(
                        formatString,
                        range.Address,
                        range.Address + (range.Size - 1),
                        range.DesiredCrc,
                        range.ActualCrc,
                        range.DesiredCrc == range.ActualCrc ? "Same" : "Different",
                        range.Type));
            }

            logger.StatusUpdateActivity(string.Empty);

            await this.vehicle.SendToolPresentNotification();

            foreach (MemoryRange range in this.ranges)
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

            this.vehicle.ClearDeviceMessageQueue();

            return successForAllRanges;
        }
    }
}
