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

    /// <summary>
    /// We expose this to users as a way to halt/exit the kernel, but it 
    /// is also a convenient place to insert code to test kernel features
    /// that are under development.
    /// </summary>
    public partial class Vehicle
    {
        /// <summary>
        /// For testing prototype kernels. 
        /// </summary>
        public async Task<bool> ExitKernel(bool kernelRunning, bool recoveryMode, CancellationToken cancellationToken, Stream unused)
        {
            try
            {
                this.device.ClearMessageQueue();

                Response<byte[]> response = await LoadKernelFromFile("test-kernel.bin");
                if (response.Status != ResponseStatus.Success)
                {
                    // The cleanup code in the finally block will exit the kernel.
                    return true;
                }

                logger.AddUserMessage("Test kernel found.");

                UInt32 kernelVersion = 0;
                int keyAlgorithm = 1; // default, will work for most factory operating systems.
                Response<uint> osidResponse = await this.QueryOperatingSystemId(cancellationToken);
                if (osidResponse.Status != ResponseStatus.Success)
                {
                    kernelVersion = await this.GetKernelVersion();

                    // TODO: Check for recovery mode.                    
                    // TODO: Load the tiny kernel, then use that to load the test kernel.
                    // Just to see whether we could potentially use that technique to assist 
                    // a user whose flash is corrupted and regular flashing isn't working.
                }
                else
                {
                    PcmInfo pi = new PcmInfo(osidResponse.Value);
                    keyAlgorithm = pi.KeyAlgorithm;
                }

                this.logger.AddUserMessage("Unlocking PCM...");
                bool unlocked = await this.UnlockEcu(keyAlgorithm);
                if (!unlocked)
                {
                    this.logger.AddUserMessage("Unlock was not successful.");
                    return false;
                }

                this.logger.AddUserMessage("Unlock succeeded.");

                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                PcmInfo info = new PcmInfo(12202088); // todo, make selectable
                if (!await PCMExecute(info, response.Value, cancellationToken))
                {
                    logger.AddUserMessage("Failed to upload kernel to PCM");

                    return false;
                }

                logger.AddUserMessage("Kernel uploaded to PCM succesfully.");

                //await this.InvestigateDataCorruption(cancellationToken);
                //await this.InvestigateKernelVersionQueryTiming();
                //await this.InvestigateCrc(cancellationToken);
                //await this.InvestigateDataRelayCorruption(cancellationToken);
                await this.InvestigateFlashChipId();
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
                this.logger.AddUserMessage("Halting kernel.");
                await this.Cleanup();
            }
        }

        /// <summary>
        /// Send a series of increasingly longer messages. They should be echoed back with
        /// only the mode byte changing from 3E to 7E.
        /// </summary>
        private async Task InvestigateDataRelayCorruption(CancellationToken cancellationToken)
        {
            for (int length = 1000; length < 1030; length++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                byte[] message = new byte[length + 5];
                message[0] = 0x6C;
                message[1] = 0x10;
                message[2] = 0xF0;
                message[3] = 0x3E;
                message[4] = (byte)length; // to indicate what the loop counter is

                for (int index = 0; index < length; index++)
                {
                    message[5 + index] = (byte)(0xAA + (0x11 * (index % 4)));
                }

                await this.device.SendMessage(new Message(message));
                await Task.Delay(3000, cancellationToken);
            }
        }

        /// <summary>
        /// This was used to test the kernel's CRC code.
        /// </summary>
        private async Task InvestigateCrc(CancellationToken cancellationToken)
        {
            ICollection<MemoryRange> ranges = FlashChip.Create(0x00894471, this.logger).MemoryRanges;

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
                Message query = this.protocol.CreateCrcQuery(range.Address, range.Size);
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
                Message vq = this.protocol.CreateKernelVersionQuery();
                await this.device.SendMessage(vq);
                Message vr = await this.device.ReceiveMessage();
                if (vr != null)
                {
                    Response<UInt32> resp = this.protocol.ParseKernelVersion(vr);
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
        /// This was used to test the delays in the kernel prior to sending a response.
        /// </summary>
        /// <returns></returns>
        private async Task InvestigateFlashChipId()
        {
            await this.device.SetTimeout(TimeoutScenario.ReadProperty);

            int successRate = 0;
            for (int attempts = 0; attempts < 1; attempts++)
            {
                Message vq = this.protocol.CreateFlashMemoryTypeQuery();
                await this.device.SendMessage(vq);
                Message responseMessage = await this.device.ReceiveMessage();
                if (responseMessage != null)
                {
                    Response<UInt32> resp = this.protocol.ParseFlashMemoryType(responseMessage);
                    if (resp.Status == ResponseStatus.Success)
                    {
                        this.logger.AddUserMessage("Flash chip ID: " + resp.Value.ToString("X8"));
                        this.logger.AddDebugMessage("Got Kernel Version");
                        successRate++;
                    }
                }

                await Task.Delay(0);
                continue;
            }
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

                Message blockMessage = protocol.CreateBlockMessage(
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

                    if (await this.WaitForSuccess(this.protocol.ParseUploadResponse, cancellationToken))
                    {
                        break;
                    }

                    this.logger.AddDebugMessage("WritePayload: Upload request failed.");
                }
            }
        }
    }
}