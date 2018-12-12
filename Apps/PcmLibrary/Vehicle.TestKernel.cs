using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PcmHacking
{
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
                    response = await LoadKernelFromFile("read-kernel.bin");
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
                    if (!await PCMExecute(response.Value, 0xFF9000, cancellationToken))
                    {
                        logger.AddUserMessage("Failed to upload kernel to PCM");

                        return false;
                    }

                    logger.AddUserMessage("Kernel uploaded to PCM succesfully.");
                }



                // Test the read kernel.
                byte[] image = new byte[512 * 1024];
                await this.device.SetTimeout(TimeoutScenario.ReadMemoryBlock);
                for (int attempts = 0; attempts < 1; attempts++)
                {
                    if (await TryReadBlock(image, this.device.MaxReceiveSize - 12, 0, cancellationToken))
                    {
                        this.logger.AddUserMessage("Read a block from ROM");
                        break;
                    }
                }



                // Load the test kernel into RAM.
                response = await LoadKernelFromFile("micro-kernel.bin");
                if (response.Status != ResponseStatus.Success)
                {
                    logger.AddUserMessage("Failed to load kernel from file.");
                    return false;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                await Task.Delay(500);

                if (!await PCMExecute(response.Value, 0xFFA000, cancellationToken))
                {
                    logger.AddUserMessage("Failed to upload kernel to PCM");

                    return false;
                }

                logger.AddUserMessage("Kernel uploaded to PCM succesfully.");


                // Test the test kernel
                
                for (int attempts = 0; attempts < 5; attempts++)
                {
                    if (await this.TryWaitForKernel(cancellationToken, 1))
                    {
                        this.logger.AddUserMessage("Received ping response.");
                        break;
                    }
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
                await TryWriteKernelReset(cancellationToken);
                await this.Cleanup();
            }
        }

        public async Task<bool> TryWaitForKernel(CancellationToken cancellationToken, int maxAttempts)
        {
            logger.AddUserMessage("Waiting for kernel to respond.");

            return await this.SendMessageValidateResponse(
                this.messageFactory.CreateKernelPing(),
                this.messageParser.ParseKernelPingResponse,
                "kernel ping",
                "Kernel is responding.",
                "No response received from the flash kernel.",
                cancellationToken,
                maxAttempts,
                false);
        }
       

        private async Task<bool> TryWriteKernelReset(CancellationToken cancellationToken)
        {
            return await this.SendMessageValidateResponse(
                this.messageFactory.CreateWriteKernelResetRequest(),
                this.messageParser.ParseWriteKernelResetResponse,
                "flash-kernel PCM reset request",
                "PCM reset.",
                "Unable to reset the PCM.",
                cancellationToken);
        }


        private async Task<bool> TryFlashUnlockAndErase(CancellationToken cancellationToken)
        {
            await this.device.SetTimeout(TimeoutScenario.Maximum);

            // These two messages must be sent in quick succession.
            // The responses may be delayed, which makes acknowledgement hard.

            //            this.logger.AddUserMessage("Unlocking and erasing calibration.");
            await this.device.SendMessage(this.messageFactory.CreateFlashUnlockRequest());
            await this.device.SendMessage(this.messageFactory.CreateCalibrationEraseRequest());
            // await this.device.SendMessage(new Message(System.Text.Encoding.ASCII.GetBytes("AT MA")));

            // Just assume success for now?
            return true;

            /*
            for (int sendAttempt = 1; sendAttempt <= 5; sendAttempt++)
            {
                // These two messages must be sent in quick succession.
                await this.device.SendMessage(this.messageFactory.CreateFlashUnlockRequest());
                await this.device.SendMessage(this.messageFactory.CreateCalibrationEraseRequest());
                for (int receiveAttempt = 1; receiveAttempt <= 5; receiveAttempt++)
                {
                    Message message = await this.device.ReceiveMessage();
                    if (message == null)
                    {
                        continue;
                    }
                    Response<bool> response = this.messageParser.ParseFlashKernelSuccessResponse(message);
                    if (response.Status != ResponseStatus.Success)
                    {
                        this.logger.AddDebugMessage("Ignoring message: " + response.Status);
                        continue;
                    }
                    this.logger.AddDebugMessage("Found response, " + (response.Value ? "succeeded." : "failed."));
                    return true;
                }
            }
            */

        }

        private async Task<bool> TryFlashLock(CancellationToken cancellationToken)
        {
            return await this.SendMessageValidateResponse(
                this.messageFactory.CreateFlashLockRequest(),
                this.messageParser.ParseFlashLockResponse,
                "flash lock request",
                "Flash memory locked.",
                "Unable to lock flash memory.",
                cancellationToken);
        }
    }
}
