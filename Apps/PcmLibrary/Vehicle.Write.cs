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

                    Response<byte[]> response = await LoadKernelFromFile("js-kernel.bin");
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

                byte[] image = new byte[stream.Length];
                int bytesRead = await stream.ReadAsync(image, 0, (int)stream.Length);
                if (bytesRead != stream.Length)
                {
                    this.logger.AddUserMessage("Unable to read image file.");
                    return false;
                }

                switch (writeType)
                {
                    case WriteType.Calibration:
                        await this.CalibrationWrite(cancellationToken, image);
                        break;

                    case WriteType.OsAndCalibration:
                        await this.OsAndCalibrationWrite(cancellationToken, image);
                        break;

                    case WriteType.Full:
                        await this.FullWrite(cancellationToken, image);
                        break;
                }

                await TryWriteKernelReset();
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

        private async Task FullWrite(CancellationToken cancellationToken, byte[] image)
        {
            Message start = new Message(new byte[] { 0x6C, 0x10, 0xF0, 0x3C, 0x01 });

            // This command begins a full flash.
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
            byte[] header = new byte[] { 0x6C, 0x10, 0x0F0, 0x36, 0x00, 0x00, chunkSize, 0xFF, 0xA0, 0x00 };
            byte[] messageBytes = new byte[header.Length + chunkSize + 2];

            for (int startAt = 0; startAt < image.Length; startAt += chunkSize)
            {
                // TODO: Move this functionality into the Message class.
                Buffer.BlockCopy(header, 0, messageBytes, 0, header.Length);
                Buffer.BlockCopy(image, startAt, messageBytes, header.Length, chunkSize);
                Vehicle.AddBlockChecksum(messageBytes);
                Message message = new Message(messageBytes);

                this.device.ClearMessageQueue();
                if (!await this.SendMessageValidateResponse(
                    message,
                    this.messageParser.ParseChunkWriteResponse,
                    this.messageParser.ParseJsKernelProcessingMessage,
                    string.Format("data from {0} to {1}", startAt, startAt + chunkSize),
                    "Data chunk sent.",
                    "Unable to send data chunk."))
                {
                    return;
                }
            }
        }

        private async Task OsAndCalibrationWrite(CancellationToken cancellationToken, byte[] image)
        {
            await Task.Delay(1);
        }

        private async Task CalibrationWrite(CancellationToken cancellationToken, byte[] image)
        {
            byte chunkSize = 192;
            byte[] header = new byte[] { 0x6C, 0x10, 0x0F0, 0x36, 0x00, 0x00, chunkSize, 0xFF, 0xA0, 0x00 };
            byte[] messageBytes = new byte[header.Length + chunkSize + 2];

            for (int startAt = 0x8000; startAt < 0x1FFFF; startAt += chunkSize)
            {
                // TODO: Move this functionality into the Message class.
                Buffer.BlockCopy(header, 0, messageBytes, 0, header.Length);
                Buffer.BlockCopy(image, startAt, messageBytes, header.Length, chunkSize);
                Vehicle.AddBlockChecksum(messageBytes);
                Message message = new Message(messageBytes);

                this.device.ClearMessageQueue();
                if (!await this.SendMessageValidateResponse(
                    message,
                    this.messageParser.ParseChunkWriteResponse,
                    this.messageParser.ParseJsKernelProcessingMessage,
                    string.Format("data from {0} to {1}", startAt, startAt + chunkSize),
                    "Data chunk sent.",
                    "Unable to send data chunk."))
                {
                    return;
                }
            }
        }
        
        public static byte[] AddBlockChecksum(byte[] Block)
        {
            UInt16 Sum = 0;

            for (int i = 4; i < Block.Length - 2; i++) // skip prio, dest, src, mode
            {
                Sum += Block[i];
            }

            Block[Block.Length - 2] = unchecked((byte)(Sum >> 8));
            Block[Block.Length - 1] = unchecked((byte)(Sum & 0xFF));

            return Block;
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
