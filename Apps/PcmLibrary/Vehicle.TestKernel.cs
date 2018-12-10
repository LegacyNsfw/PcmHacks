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
            byte[] image = new byte[0];// stream.Length];

            try
            {
                this.device.ClearMessageQueue();

                if (!kernelRunning)
                {        
                    Response<byte[]> response = await LoadKernelFromFile("micro-kernel.bin");
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
                
                // TryWaitForKernel will log user messages.
//                if (await this.TryWaitForKernel(5))
                {
                    try
                    {
                        if (!await this.TryFlashUnlockAndErase(cancellationToken))
                        {
                            return false;
                        }
                    }
                    finally
                    {
                        await this.TryFlashLock(cancellationToken);
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

        public async Task<bool> TryWaitForKernel(int maxAttempts, CancellationToken cancellationToken)
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
