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


                await this.InvestigateDataCorruption(cancellationToken);
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
