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
        public async Task<bool> TestKernel(bool kernelRunning, bool recoveryMode, CancellationToken cancellationToken, Stream stream)
        {
            try
            {
                this.device.ClearMessageQueue();
                Response<byte[]> response;

                if (!kernelRunning)
                {        
                    response = await LoadKernelFromFile("write-kernel.bin");
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

                // Which flash chip?
                Query<UInt32> chipIdQuery = new Query<uint>(
                    this.device,
                    this.messageFactory.CreateFlashMemoryTypeQuery,
                    this.messageParser.ParseFlashMemoryType,
                    this.logger,
                    cancellationToken);
                Response<UInt32> chipIdResponse = await chipIdQuery.Execute();

                this.logger.AddUserMessage("Flash chip ID = " + chipIdResponse.Value.ToString("X8"));

                await this.device.SendMessage(new Message(new byte[] { 0x6C, 0x10, 0xF0, 0x3D, 0x02 }));

                await Task.Delay(100);

                for(int i = 1; i < 20; i++)
                {
                    Message rx;
                    while ((rx = await this.device.ReceiveMessage()) != null)
                    {
                        this.logger.AddUserMessage(rx.ToString());
                    }

                    await Task.Delay(100);
                }

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
    }
}
