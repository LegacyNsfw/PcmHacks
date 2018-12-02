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
        /// Replace the full contents of the PCM.
        /// </summary>
        public async Task<bool> Write(bool fullWrite, bool kernelRunning, bool recoveryMode, CancellationToken cancellationToken, Stream stream)
        {
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
                    if (!await PCMExecute(response.Value, 0xFF81FE, cancellationToken))
                    {
                        logger.AddUserMessage("Failed to upload kernel to PCM");

                        return false;
                    }

//                    await toolPresentNotifier.Notify();

                    logger.AddUserMessage("Kernel uploaded to PCM succesfully.");
                }

                await this.device.SetTimeout(TimeoutScenario.Maximum);

                if (fullWrite)
                {
                    await this.FullWrite(cancellationToken, stream);
                }
                else
                {
                    await this.CalibrationWrite(cancellationToken, stream);
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
//                await TryWriteKernelReset();
//                await this.Cleanup();
            }
        }
        
        private async Task FullWrite(CancellationToken cancellationToken, Stream stream)
        {
            Message start = new Message(new byte[] { 0x6C, 0x10, 0xF0, 0x3C, 0x01 });

            if (!await this.SendMessageValidateResponse(
                start,
                this.messageParser.ParseStartFullFlashResponse,
                null,
                "start full flash",
                "Full flash starting.",
                "Kernel won't allow a full flash."))
            {
                return;
            }
            
            byte chunkSize = 192;
            byte[] payload = new byte[chunkSize];
            for (int bytesSent = 0; bytesSent < stream.Length; bytesSent += chunkSize)
            {
                int bytesRead = stream.Read(payload, 0, chunkSize);
                Message message = this.messageFactory.CreateBlockMessage(payload, 0, chunkSize, 0xFFA000, false);
                
                 if (!await this.SendMessageValidateResponse(
                    message,
                    this.messageParser.ParseChunkWriteResponse,
                    this.messageParser.ParseJsKernelProcessingMessage,
                    string.Format("chunk upload ({0} to {1})", bytesSent, bytesSent + chunkSize),
                    "Data chunk sent.",
                    "Unable to send data chunk."))
                {
                    return;
                }
            }
        }

        private Task CalibrationWrite(CancellationToken cancellationToken, Stream stream)
        {
            return Task.FromResult(0);
        }
       
        
        private async Task<bool> SendMessageValidateResponse(
            Message message,
            Func<Message, Response<bool>> requiredMessageFilter,
            Func<Message, Response<bool>> expectedMessageFilter,
            string messageDescription,
            string successMessage,
            string failureMessage,
            int maxAttempts = 5,
            bool pingKernel = false)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                this.logger.AddUserMessage("Sending " + messageDescription);

                if (!await this.TrySendMessage(message, messageDescription, maxAttempts))
                {
                    this.logger.AddUserMessage("Unable to send " + messageDescription);
                    if (pingKernel)
                    {
                        await this.TryWaitForKernel(1);
                    }
                    continue;
                }

                if (!await this.WaitForSuccess(requiredMessageFilter, expectedMessageFilter, 10))
                {
                    this.logger.AddUserMessage("No " + messageDescription + " response received.");
                    if (pingKernel)
                    {
                        await this.TryWaitForKernel(1);
                    }
                    continue;
                }

                this.logger.AddUserMessage(successMessage);
                return true;
            }

            this.logger.AddUserMessage(failureMessage);
            if (pingKernel)
            {
                await this.TryWaitForKernel(1);
            }
            return false;
        }
    }
}
