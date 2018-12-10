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
    /// How much of the PCM to erase and rewrite.
    /// </summary>
    public enum WriteType
    {
        Invalid = 0,
        Calibration = 1,
        OsAndCalibration = 2,
        Full = 3,
    }

    public partial class Vehicle
    {
        /// <summary>
        /// Replace the full contents of the PCM.
        /// </summary>
        public async Task<bool> Write(WriteType writeType, bool kernelRunning, bool recoveryMode, CancellationToken cancellationToken, Stream stream)
        {
            byte[] image = new byte[stream.Length];
            int bytesRead = await stream.ReadAsync(image, 0, (int)stream.Length);
            if (bytesRead != stream.Length)
            {
                this.logger.AddUserMessage("Unable to read input file.");
                return false;
            }

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
                    if (!await PCMExecute(response.Value, 0xFF9106, cancellationToken))
                    {
                        logger.AddUserMessage("Failed to upload kernel to PCM");

                        return false;
                    }

//                    await toolPresentNotifier.Notify();

                    logger.AddUserMessage("Kernel uploaded to PCM succesfully.");
                }

                await this.device.SetTimeout(TimeoutScenario.Maximum);

                switch (writeType)
                {
                    case WriteType.Calibration:
                        await this.CalibrationWrite(cancellationToken, stream);
                        break;

                    case WriteType.OsAndCalibration:
                        await this.OsAndCalibrationWrite(cancellationToken, stream);
                        break;

                    case WriteType.Full:
                        await this.FullWrite(cancellationToken, stream);
                        break;
                }

                await TryWriteKernelReset(cancellationToken);
                await this.Cleanup();
                return true;
            }
            catch (Exception exception)
            {
                this.logger.AddUserMessage("Something went wrong. " + exception.Message);
                this.logger.AddUserMessage("Do not power off the PCM! Do not exit this program!");
                this.logger.AddUserMessage("Try flashing again. If errors continue, seek help online.");
                this.logger.AddUserMessage("https://pcmhacking.net/forums/viewtopic.php?f=3&t=6080");
                this.logger.AddDebugMessage(exception.ToString());
                return false;
            }
        }

        private Task CalibrationWrite(CancellationToken cancellationToken, Stream stream)
        {
            return Task.FromResult(0);
        }

        private Task OsAndCalibrationWrite(CancellationToken cancellationToken, Stream stream)
        {
            return Task.FromResult(0);
        }

        private async Task FullWrite(CancellationToken cancellationToken, Stream stream)
        {
            Message start = new Message(new byte[] { 0x6C, 0x10, 0xF0, 0x3C, 0x01 });

            if (!await this.SendMessageValidateResponse(
                start,
                this.messageParser.ParseStartFullFlashResponse,
                "start full flash",
                "Full flash starting.",
                "Kernel won't allow a full flash.",
                cancellationToken))
            {
                return;
            }
            
            byte chunkSize = 192;
            byte[] header = new byte[] { 0x6C, 0x10, 0x0F0, 0x3C, 0x00, 0x00, chunkSize, 0xFF, 0xA0, 0x00 };
            byte[] messageBytes = new byte[header.Length + chunkSize + 2];
            Buffer.BlockCopy(header, 0, messageBytes, 0, header.Length);
            for (int bytesSent = 0; bytesSent < stream.Length; bytesSent += chunkSize)
            {
                stream.Read(messageBytes, header.Length, chunkSize);
                VPWUtils.AddBlockChecksum(messageBytes); // TODO: Move this function into the Message class.
                Message message = new Message(messageBytes);

                if (!await this.SendMessageValidateResponse(
                    message,
                    this.messageParser.ParseChunkWriteResponse,
                    string.Format("data from {0} to {1}", bytesSent, bytesSent + chunkSize),
                    "Data chunk sent.",
                    "Unable to send data chunk.",
                    cancellationToken))
                {
                    return;
                }
            }
        }

        private async Task<bool> SendMessageValidateResponse(
            Message message,
            Func<Message, Response<bool>> filter,
            string messageDescription,
            string successMessage,
            string failureMessage,
            CancellationToken cancellationToken,
            int maxAttempts = 5,
            bool pingKernel = false)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                this.logger.AddUserMessage("Sending " + messageDescription);

                if (!await this.TrySendMessage(message, messageDescription, maxAttempts))
                {
                    this.logger.AddUserMessage("Unable to send " + messageDescription);
                    if (pingKernel)
                    {
                        await this.TryWaitForKernel(cancellationToken, 1);
                    }
                    continue;
                }

                if (!await this.WaitForSuccess(filter, cancellationToken, 10))
                {
                    this.logger.AddUserMessage("No " + messageDescription + " response received.");
                    if (pingKernel)
                    {
                        await this.TryWaitForKernel(cancellationToken, 1);
                    }
                    continue;
                }

                this.logger.AddUserMessage(successMessage);
                return true;
            }

            this.logger.AddUserMessage(failureMessage);
            if (pingKernel)
            {
                await this.TryWaitForKernel(cancellationToken, 1);
            }
            return false;
        }
    }
}
