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
        private readonly ILogger logger;

        public CKernelVerifier(byte[] image, IEnumerable<MemoryRange> ranges, Vehicle vehicle, Protocol protocol, ILogger logger)
        {
            this.image = image;
            this.ranges = ranges;
            this.vehicle = vehicle;
            this.protocol = protocol;
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
                range.DesiredCrc = crc.GetCrc(this.image, range.Address, range.Size);
            }
        }

        /// <summary>
        /// Compare CRCs from the file to CRCs from the PCM.
        /// </summary>
        public async Task<bool> CompareRanges(
            IList<MemoryRange> ranges, 
            byte[] image, 
            BlockType blockTypes,
            CancellationToken cancellationToken)
        {
            logger.AddUserMessage("Calculating CRCs from file...");
            this.GetCrcFromImage();

            logger.AddUserMessage("Requesting CRCs from PCM...");
            await this.vehicle.SendToolPresentNotification();

            // The kernel will remember (and return) the CRC value of the last block it 
            // was asked about, which leads to misleading results if you only rewrite 
            // a single block. So we send a a bogus query to reset the last-used CRC 
            // value in the kernel.
            Query<UInt32> crcReset = this.vehicle.CreateQuery<uint>(
                () => this.protocol.CreateCrcQuery(0, 0),
                (message) => this.protocol.ParseCrc(message, 0, 0),
                cancellationToken);
            await crcReset.Execute();

            await this.vehicle.SetDeviceTimeout(TimeoutScenario.ReadCrc);
            bool successForAllRanges = true;

            logger.AddUserMessage("\tRange\t\tFile CRC\t\tPCM CRC\tVerdict\tPurpose");
            foreach (MemoryRange range in ranges)
            {
                string formatString = "{0:X6}-{1:X6}\t{2:X8}\t{3:X8}\t{4}\t{5}";

                if ((range.Type & blockTypes) == 0)
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
                
                // You might think that a shorter retry delay would speed things up,
                // but 1500ms delay gets CRC results in about 3.5 seconds.
                // A 1000ms delay resulted in 4+ second CRC responses, and a 750ms
                // delay resulted in 5 second CRC responses. The PCM needs to spend
                // its time caculating CRCs rather than responding to messages.
                int retryDelay = 1500;
                Message query = this.protocol.CreateCrcQuery(range.Address, range.Size);
                for (int attempts = 0; attempts < 10; attempts++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    await this.vehicle.SendToolPresentNotification();
                    if (!await this.vehicle.SendMessage(query))
                    {
                        // This delay is fast because we're waiting for the bus to be available,
                        // rather than waiting for the PCM's CPU to finish computing the CRC as 
                        // with the other two delays below.
                        await Task.Delay(100);
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

            await this.vehicle.SendToolPresentNotification();

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

            this.vehicle.ClearDeviceMessageQueue();

            return successForAllRanges;
        }
    }
}
