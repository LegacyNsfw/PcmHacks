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
    /// Describes the type of information in a flash block.
    /// </summary>
    [Flags]
    public enum BlockType
    {
        Invalid = 0,
        Boot = 1,
        Parameter = 2,
        Calibration = 4,
        OperatingSystem = 8,
        All = 15
    };

    /// <summary>
    /// Defines a block of flash memory.
    /// </summary>
    public class MemoryRange
    {
        public UInt32 Address { get; private set; }
        public UInt32 Size { get; private set; }
        public BlockType Type { get; private set; }
        public UInt32 DesiredCrc { get; set; }
        public UInt32 ActualCrc { get; set; }

        public MemoryRange(UInt32 address, UInt32 size, BlockType type)
        {
            this.Address = address;
            this.Size = size;
            this.Type = type;
        }
    }

    public partial class Vehicle
    {        
        /// <summary>
        /// For testing prototype kernels. 
        /// </summary>
        public async Task<bool> TestKernel(bool kernelRunning, bool recoveryMode, CancellationToken cancellationToken, Stream unused)
        {
            try
            {
                this.device.ClearMessageQueue();                

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


                //await this.InvestigateDataCorruption(cancellationToken);
                //await this.InvestigateKernelVersionQueryTiming();
                await this.InvestigateCrc(cancellationToken);
                return true;
            }
            catch (Exception exception)
            {
                this.logger.AddUserMessage("Something went wrong. " + exception.Message);
                this.logger.AddDebugMessage(exception.ToString());
                return false;
            }
            finally
            {
                await this.Cleanup();
            }
        }

        private async Task InvestigateCrc(CancellationToken cancellationToken)
        {
            IList<MemoryRange> ranges = this.GetMemoryRanges(0x00894471);

            logger.AddUserMessage("Requesting CRCs from PCM...");
            foreach (MemoryRange range in ranges)
            {
                this.device.ClearMessageQueue();
                bool success = false;
                UInt32 crc = 0;
                await this.device.SetTimeout(TimeoutScenario.ReadCrc);

                // You might think that a shorter retry delay would speed things up,
                // but 1500ms delay gets CRC results in about 3.5 seconds.
                // A 1000ms delay resulted in 4+ second CRC responses, and a 750ms
                // delay resulted in 5 second CRC responses. The PCM needs to spend
                // its time caculating CRCs rather than responding to messages.
                int retryDelay = 1500;
                Message query = this.messageFactory.CreateCrcQuery(range.Address, range.Size);
                for (int attempts = 0; attempts < 100; attempts++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (!await this.device.SendMessage(query))
                    {
                        await Task.Delay(retryDelay);
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
                    continue;
                }

                range.ActualCrc = crc;

                this.logger.AddUserMessage(
                    string.Format(
                        "Range {0:X6}-{1:X6} - Local: {2:X8} - PCM: {3:X8} - {4}",
                        range.Address,
                        range.Address + (range.Size - 1),
                        range.DesiredCrc,
                        range.ActualCrc,
                        range.Type));
            }

        }

        /// <summary>
        /// This was used to test the delays in the kernel prior to sending a response.
        /// </summary>
        /// <returns></returns>
        private async Task InvestigateKernelVersionQueryTiming()
        {
            await this.device.SetTimeout(TimeoutScenario.ReadProperty);

            int successRate = 0;
            for (int attempts = 0; attempts < 100; attempts++)
            {
                Message vq = this.messageFactory.CreateKernelVersionQuery();
                await this.device.SendMessage(vq);
                Message vr = await this.device.ReceiveMessage();
                if (vr != null)
                {
                    Response<UInt32> resp = this.messageParser.ParseKernelVersion(vr);
                    if (resp.Status == ResponseStatus.Success)
                    {
                        this.logger.AddDebugMessage("Got Kernel Version");
                        successRate++;
                    }
                }

                await Task.Delay(0);
                continue;
            }

            this.logger.AddDebugMessage("Success rate: " + successRate.ToString());
        }

        /// <summary>
        /// AllPro was getting data corruption when payloads got close to 2kb. 
        /// Still not sure what's up with that. 
        /// Using 1kb payloads until we figure that out.
        /// </summary>
        private async Task InvestigateDataCorruption(CancellationToken cancellationToken)
        {
            await this.device.SetTimeout(TimeoutScenario.Maximum);

            for (int length = 2010; length < 2055; length += 16)
            //for (int iterations = 0; iterations < 100; iterations++)
            {
                //int length = 2040;
                byte[] payload = new byte[length];
                for (int index = 0; index < length; index++)
                {
                    payload[index] = 1;
                }

                Message blockMessage = messageFactory.CreateBlockMessage(
                    payload,
                    0,
                    length,
                    0xFFB000,
                    BlockCopyType.Copy);

                for (int i = 3; i > 0; i--)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (!await device.SendMessage(blockMessage))
                    {
                        this.logger.AddDebugMessage("WritePayload: Unable to send message.");
                        continue;
                    }

                    if (await this.WaitForSuccess(this.messageParser.ParseUploadResponse, cancellationToken))
                    {
                        break;
                    }

                    this.logger.AddDebugMessage("WritePayload: Upload request failed.");
                }
            }
        }
    }
}
