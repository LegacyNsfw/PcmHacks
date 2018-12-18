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

                // Give the kernel time to start.
                await Task.Delay(500);

                // Which kernel?
                Query<UInt32> versionQuery = new Query<uint>(
                    this.device,
                    this.messageFactory.CreateKernelVersionQuery,
                    this.messageParser.ParseKernelVersion,
                    this.logger,
                    cancellationToken);
                Response<UInt32> versionResponse = await versionQuery.Execute();

                this.logger.AddUserMessage("Version = " + versionResponse.Value.ToString("X8"));
/*
                // Which flash chip?
                Query<UInt32> chipIdQuery = new Query<uint>(
                    this.device,
                    this.messageFactory.CreateFlashMemoryTypeQuery,
                    this.messageParser.ParseFlashMemoryType,
                    this.logger,
                    cancellationToken);
                Response<UInt32> chipIdResponse = await chipIdQuery.Execute();

                this.logger.AddUserMessage("Flash chip ID = " + chipIdResponse.Value.ToString("X8"));
                */

                Query<byte> unlockRequest = new Query<byte>(
                    this.device,
                    this.messageFactory.CreateFlashUnlockRequest,
                    this.messageParser.ParseFlashUnlock,
                    this.logger,
                    cancellationToken);
                Response<byte> unlockResponse = await unlockRequest.Execute();
                

                //await chipIdQuery.Execute();
                await versionQuery.Execute();
                this.logger.AddUserMessage("Version = " + versionResponse.Value.ToString("X8"));
                await versionQuery.Execute();
                this.logger.AddUserMessage("Version = " + versionResponse.Value.ToString("X8"));
                await versionQuery.Execute();
                this.logger.AddUserMessage("Version = " + versionResponse.Value.ToString("X8"));
                await versionQuery.Execute();
                this.logger.AddUserMessage("Version = " + versionResponse.Value.ToString("X8"));
                await versionQuery.Execute();
                this.logger.AddUserMessage("Version = " + versionResponse.Value.ToString("X8"));
                await versionQuery.Execute();
                this.logger.AddUserMessage("Version = " + versionResponse.Value.ToString("X8"));
                await versionQuery.Execute();
                this.logger.AddUserMessage("Version = " + versionResponse.Value.ToString("X8"));
                await versionQuery.Execute();
                this.logger.AddUserMessage("Version = " + versionResponse.Value.ToString("X8"));
                await versionQuery.Execute();
                this.logger.AddUserMessage("Version = " + versionResponse.Value.ToString("X8"));
                await versionQuery.Execute();
                this.logger.AddUserMessage("Version = " + versionResponse.Value.ToString("X8"));
                await versionQuery.Execute();
                this.logger.AddUserMessage("Version = " + versionResponse.Value.ToString("X8"));
                await versionQuery.Execute();
                this.logger.AddUserMessage("Version = " + versionResponse.Value.ToString("X8"));
                await versionQuery.Execute();
                this.logger.AddUserMessage("Version = " + versionResponse.Value.ToString("X8"));

                Query<byte> lockRequest = new Query<byte>(
                    this.device,
                    this.messageFactory.CreateFlashLockRequest,
                    this.messageParser.ParseFlashLock,
                    this.logger,
                    cancellationToken);
                Response<byte> lockResponse = await lockRequest.Execute();

                await versionQuery.Execute();

                Message message = this.messageFactory.CreateDebugQuery();
                await this.device.SendMessage(message);

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
