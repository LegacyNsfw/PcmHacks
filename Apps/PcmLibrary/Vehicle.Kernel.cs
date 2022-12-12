using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// From the application's perspective, this class is the API to the vehicle.
    /// </summary>
    /// <remarks>
    /// Methods in this class are high-level operations like "get the VIN," or "read the contents of the EEPROM."
    /// </remarks>
    public partial class Vehicle : IDisposable
    {
        /// <summary>
        /// Suppres chatter on the VPW bus.
        /// </summary>
        public async Task SuppressChatter()
        {
            this.logger.AddDebugMessage("Suppressing VPW chatter.");
            Message suppressChatter = this.protocol.CreateDisableNormalMessageTransmission();
            await this.device.SendMessage(suppressChatter);
            await this.notifier.ForceNotify();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < 1000)
            {
                Message received = await this.device.ReceiveMessage();
                if (received != null)
                {
                    this.logger.AddDebugMessage("Ignoring chatter: " + received.ToString());
                    break;
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }

        /// <summary>
        /// Writes a block of data to the PCM
        /// Requires an unlocked PCM
        /// </summary>
        private async Task<Response<bool>> WriteBlock(byte block, byte[] data)
        {
            Message m;
            Message ok = new Message(new byte[] { 0x6C, DeviceId.Tool, DeviceId.Pcm, 0x7B, block });

            switch (data.Length)
            {
                case 6:
                    m = new Message(new byte[] { 0x6C, DeviceId.Pcm, DeviceId.Tool, 0x3B, block, data[0], data[1], data[2], data[3], data[4], data[5] });
                    break;
                default:
                    logger.AddDebugMessage("Cant write block size " + data.Length);
                    return Response.Create(ResponseStatus.Error, false);
            }

            if (!await this.device.SendMessage(m))
            {
                logger.AddUserMessage("Failed to write block " + block + ", communications failure");
                return Response.Create(ResponseStatus.Error, false);
            }

            logger.AddDebugMessage("Successful write to block " + block);
            return Response.Create(ResponseStatus.Success, true);
        }

        /// <summary>
        /// Opens the named kernel file. The file must be in the same directory as the EXE.
        /// </summary>
        public async Task<Response<byte[]>> LoadKernelFromFile(string path)
        {
            byte[] file = { 0x00 }; // dummy value

            if (path == "")
            {
                return Response.Create(ResponseStatus.Error, file);
            }

            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string exeDirectory = Path.GetDirectoryName(exePath);
            path = Path.Combine(exeDirectory, path);

            try
            {
                using (Stream fileStream = File.OpenRead(path))
                {
                    if (fileStream.Length == 0)
                    {
                        logger.AddDebugMessage("invalid kernel image (zero bytes). " + path);
                        return Response.Create(ResponseStatus.Error, file);
                    }
                    file = new byte[fileStream.Length];

                    // In theory we might need a loop here. In practice, I don't think that will be necessary.
                    int bytesRead = await fileStream.ReadAsync(file, 0, (int)fileStream.Length);

                    if(bytesRead != fileStream.Length)
                    {
                        return Response.Create(ResponseStatus.Truncated, file);
                    }
                }

                logger.AddDebugMessage("Loaded " + path);
            }
            catch (ArgumentException)
            {
                logger.AddDebugMessage("Invalid file path " + path);
                return Response.Create(ResponseStatus.Error, file);
            }
            catch (PathTooLongException)
            {
                logger.AddDebugMessage("File path is too long " + path);
                return Response.Create(ResponseStatus.Error, file);
            }
            catch (DirectoryNotFoundException)
            {
                logger.AddDebugMessage("Invalid directory " + path);
                return Response.Create(ResponseStatus.Error, file);
            }
            catch (IOException)
            {
                logger.AddDebugMessage("Error accessing file " + path);
                return Response.Create(ResponseStatus.Error, file);
            }
            catch (UnauthorizedAccessException)
            {
                logger.AddDebugMessage("No permission to read file " + path);
                return Response.Create(ResponseStatus.Error, file);
            }

            return Response.Create(ResponseStatus.Success, file);
        }

        /// <summary>
        /// Cleanup calls the various cleanup routines to get everything back to normal
        /// </summary>
        /// <remarks>
        /// Exit kernel at 4x, 1x, and clear DTCs
        /// </remarks>
        public async Task Cleanup()
        {
            this.logger.AddDebugMessage("Halting the kernel.");
            await this.ExitKernel();
            await this.ClearTroubleCodes();
        }

        /// <summary>
        /// Exits the kernel at 4x, then at 1x. Once this function has been called the bus will be back at 1x.
        /// </summary>
        /// <remarks>
        /// Can be used to force exit the kernel, if requied. Does not attempt the 4x exit if not supported by the current device.
        /// </remarks>
        public async Task ExitKernel()
        {
            Message exitKernel = this.protocol.CreateExitKernel();

            this.device.ClearMessageQueue();
            if (device.Supports4X)
            {
                await device.SetVpwSpeed(VpwSpeed.FourX);
                await this.device.SendMessage(exitKernel);
                await device.SetVpwSpeed(VpwSpeed.Standard);
            }

            await this.device.SendMessage(exitKernel);
        }

        /// <summary>
        /// Ask the factory operating system to clear trouble codes. 
        /// In theory this should only run 10 seconds after rebooting, to ensure that the operating system is running again.
        /// In practice, that hasn't been an issue. It's the other modules (TAC especially) that really need to be reset.
        /// </summary>
        public async Task ClearTroubleCodes()
        {
            this.logger.AddUserMessage("Clearing trouble codes.");
            this.device.ClearMessageQueue();

            // No timeout because we don't care about responses to these messages.
            await this.device.SetTimeout(TimeoutScenario.Minimum);

            // The response is not checked because the priority byte and destination address are odd.
            // Different devices will handle this differently. Scantool won't recieve it.
            // so we send it twice just to be sure.
            Message clearCodesRequest = this.protocol.CreateClearDiagnosticTroubleCodesRequest();

            await Task.Delay(250);
            await this.device.SendMessage(clearCodesRequest);
            await Task.Delay(250);
            await this.device.SendMessage(clearCodesRequest);

            // This is a conventional message, but the response from the PCM might get lost 
            // among the responses from other modules on the bus, so again we just send it twice.
            Message clearDiagnosticInformationRequest = this.protocol.CreateClearDiagnosticInformationRequest();

            await Task.Delay(250);
            await this.device.SendMessage(clearDiagnosticInformationRequest);
            await Task.Delay(250);
            await this.device.SendMessage(clearDiagnosticInformationRequest);
        }

        /// <summary>
        /// Query the PCM's operating system ID.
        /// </summary>
        /// <returns></returns>
        public async Task<Response<UInt32>> QueryOperatingSystemIdFromKernel(CancellationToken cancellationToken)
        {
            await this.device.SetTimeout(TimeoutScenario.ReadProperty);

            var query = this.CreateQuery(
                this.protocol.CreateOperatingSystemIdKernelRequest,
                this.protocol.ParseOperatingSystemIdKernelResponse,
                CancellationToken.None);

            return await query.Execute();
        }

        /// <summary>
        /// Ask the kernel for the ID of the flash chip.
        /// </summary>
        public async Task<UInt32> QueryFlashChipId(CancellationToken cancellationToken)
        {
            for (int retries = 0; retries < 3; retries++)
            {
                await this.SetDeviceTimeout(TimeoutScenario.ReadProperty);
                Query<UInt32> chipIdQuery = this.CreateQuery<UInt32>(
                    this.protocol.CreateFlashMemoryTypeQuery,
                    this.protocol.ParseFlashMemoryType,
                    cancellationToken);
                Response<UInt32> chipIdResponse = await chipIdQuery.Execute();

                if (chipIdResponse.Status != ResponseStatus.Success)
                {
                    continue;
                }

                if (chipIdResponse.Value == 0)
                {
                    continue;
                }

                return chipIdResponse.Value;
            }

            logger.AddUserMessage("Unable to determine which flash chip is in this PCM");
            return 0;
        }

        /// <summary>
        /// Check for a running kernel.
        /// </summary>
        /// <returns></returns>
        public async Task<UInt32> GetKernelVersion()
        {
            Message query = this.protocol.CreateKernelVersionQuery();
            for (int retryCount = 0; retryCount < 5; retryCount++)
            {
                if (!await this.device.SendMessage(query))
                {
                    await Task.Delay(100);
                    continue;
                }

                Message reply = await this.device.ReceiveMessage();
                if (reply == null)
                {
                    await Task.Delay(100);
                    continue;
                }

                Response<UInt32> response = this.protocol.ParseKernelVersion(reply);
                if ((response.Status == ResponseStatus.Success) && (response.Value != 0))
                {
                    return response.Value;
                }

                if (response.Status == ResponseStatus.Refused)
                {
                    return 0;
                }

                await Task.Delay(100);
            }

            return 0;
        }

        /// <summary>
        /// Load the executable payload on the PCM at the supplied address, and execute it.
        /// </summary>
        public async Task<bool> PCMExecute(PcmInfo info, byte[] payload, CancellationToken cancellationToken)
        {
            // Note that we request an upload of 4k maximum, because the PCM will reject anything bigger.
            // But you can request a 4k upload and then send up to 16k if you want, and the PCM will not object.
            int claimedSize = Math.Min(4096, payload.Length);

            // Since we're going to lie about the size, we need to check for overflow ourselves.
            if (info.HardwareType == PcmType.P01_P59)
            {
                if (info.KernelBaseAddress + payload.Length > 0xFFCDFF)
                {
                    logger.AddUserMessage("Base address and size would exceed usable RAM.");
                    return false;
                }
            }

            logger.AddDebugMessage("Sending upload request for kernel size " + payload.Length + ", loadaddress " + info.KernelBaseAddress.ToString("X6"));

            Query<bool> uploadPermissionQuery = new Query<bool>(
                this.device,
                () => protocol.CreateUploadRequest(info, claimedSize),
                (message) => protocol.ParseUploadPermissionResponse(info, message),
                this.logger,
                cancellationToken,
                this.notifier);

            Response<bool> permissionResponse = await uploadPermissionQuery.Execute();
            bool uploadAllowed = permissionResponse.Status == ResponseStatus.Success && permissionResponse.Value;

            if (!uploadAllowed)
            {
                logger.AddUserMessage("Permission to upload kernel was denied.");
                logger.AddUserMessage("If this persists, try cutting power to the PCM, restoring power, waiting ten seconds, and trying again.");
                return false;
            }

            logger.AddDebugMessage("Going to load a " + payload.Length + " byte kernel to 0x" + info.KernelBaseAddress.ToString("X6"));

            await this.device.SetTimeout(TimeoutScenario.SendKernel);

            // Loop through the payload building and sending packets, highest first, execute on last
            int payloadSize = device.MaxKernelSendSize - 12; // Headers use 10 bytes, sum uses 2 bytes.
            int chunkCount = payload.Length / payloadSize;
            int remainder = payload.Length % payloadSize;

            int offset = (chunkCount * payloadSize);
            int startAddress = info.KernelBaseAddress + offset;

            // First we send the 'remainder' payload, containing any bytes that won't fill up an entire upload packet.
            logger.AddDebugMessage(
                string.Format(
                    "Sending end block payload with offset 0x{0:X}, start address 0x{1:X}, length 0x{2:X}.",
                    offset,
                    startAddress,
                    remainder));

            Message remainderMessage = protocol.CreateBlockMessage(
                payload, 
                offset, 
                remainder, 
                info.KernelBaseAddress + offset, 
                remainder == payload.Length ? BlockCopyType.Execute : BlockCopyType.Copy);

            await notifier.Notify();
            Response<bool> uploadResponse = await WritePayload(remainderMessage, cancellationToken);
            if (uploadResponse.Status != ResponseStatus.Success)
            {
                logger.AddDebugMessage("Could not upload kernel to PCM, remainder payload not accepted.");
                return false;
            }

            // Now we send a series of full upload packets
            // Note that there's a notifier.Notify() call inside the WritePayload() call in this loop.
            for (int chunkIndex = chunkCount; chunkIndex > 0; chunkIndex--)
            {
                int bytesSent = payload.Length - offset;
                int percentDone = bytesSent * 100 / payload.Length;

                this.logger.AddUserMessage(
                    string.Format(
                        "Kernel upload {0}% complete.",
                        percentDone));

                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                offset = (chunkIndex - 1) * payloadSize;
                startAddress = info.KernelBaseAddress + offset;
                Message payloadMessage = protocol.CreateBlockMessage(
                    payload,
                    offset,
                    payloadSize,
                    startAddress,
                    offset == 0 ? BlockCopyType.Execute : BlockCopyType.Copy);

                logger.AddDebugMessage(
                    string.Format(
                        "Sending block with offset 0x{0:X6}, start address 0x{1:X6}, length 0x{2:X4}.",
                        offset,
                        startAddress,
                        payloadSize));

                uploadResponse = await WritePayload(payloadMessage, cancellationToken);
                if (uploadResponse.Status != ResponseStatus.Success)
                {
                    logger.AddDebugMessage("Could not upload kernel to PCM, payload not accepted.");
                    return false;
                }
            }

            this.logger.AddUserMessage("Kernel upload 100% complete.");

            if (ReportKernelID && info.KernelVersionSupport)
            {
                // Consider: Allowing caller to call GetKernelVersion(...)?
                // Consider: return kernel version rather than boolean?
                UInt32 kernelVersion = await this.GetKernelVersion();
                this.logger.AddUserMessage("Kernel Version: " + kernelVersion.ToString("X8"));
            }

            return true;
        }

        /// <summary>
        /// Does everything required to switch to VPW 4x
        /// </summary>
        public async Task<bool> VehicleSetVPW4x(VpwSpeed newSpeed)
        {
            if (!device.Supports4X) 
            {
                if (newSpeed == VpwSpeed.FourX)
                {
                    // where there is no support only report no switch to 4x
                    logger.AddUserMessage("This interface does not support VPW 4x");
                }
                return true;
            }

            if ((newSpeed == VpwSpeed.FourX) && !this.device.Enable4xReadWrite)
            {
                logger.AddUserMessage("4X communications disabled by configuration.");
                return true;
            }

            // Configure the vehicle bus when switching to 4x
            if (newSpeed == VpwSpeed.FourX)
            {
                logger.AddUserMessage("Attempting switch to VPW 4x");
                await device.SetTimeout(TimeoutScenario.ReadProperty);

                // The list of modules may not be useful after all, but 
                // checking for an empty list indicates an uncooperative
                // module on the VPW bus.
                List<byte> modules = await this.RequestHighSpeedPermission(notifier);
                if (modules == null)
                {
                    // A device has refused the switch to high speed mode.
                    return false;
                }

                // Since we had some issue with other modules not staying quiet...
                await this.ForceSendToolPresentNotification();

                Message broadcast = this.protocol.CreateBeginHighSpeed(DeviceId.Broadcast);
                await this.device.SendMessage(broadcast);

                // Check for any devices that refused to switch to 4X speed.
                // These responses usually get lost, so this code might be pointless.
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Message response = null;

                // WARNING: The AllPro stopped receiving permission-to-upload messages when this timeout period
                // was set to 1500ms.  Reducing it to 500 seems to have fixed that problem. 
                // 
                // It would be nice to find a way to wait equally long with all devices, as refusal messages
                // are still a potetial source of trouble. 
                while (((response = await this.device.ReceiveMessage()) != null) && (sw.ElapsedMilliseconds < 500))
                {
                    Response<bool> refused = this.protocol.ParseHighSpeedRefusal(response);
                    if (refused.Status != ResponseStatus.Success)
                    {
                        // This should help ELM devices receive responses.
                        await Task.Delay(100);
                        await notifier.ForceNotify();
                        continue;
                    }

                    if (refused.Value == false)
                    {
                        // TODO: Add module number.
                        this.logger.AddUserMessage("Module refused high-speed switch.");
                        return false;
                    }
                }
            }
            else
            {
                logger.AddUserMessage("Reverting to VPW 1x");
            }

            // Request the device to change
            await device.SetVpwSpeed(newSpeed);

            // Since we had some issue with other modules not staying quiet...
            await this.ForceSendToolPresentNotification();

            return true;
        }

        /// <summary>
        /// Ask all of the devices on the VPW bus for permission to switch to 4X speed.
        /// </summary>
        private async Task<List<byte>> RequestHighSpeedPermission(ToolPresentNotifier notifier)
        {
            Message permissionCheck = this.protocol.CreateHighSpeedPermissionRequest(DeviceId.Broadcast);
            await this.device.SendMessage(permissionCheck);

            // Note that as of right now, the AllPro only receives 6 of the 11 responses.
            // So until that gets fixed, we could miss a 'refuse' response and try to switch
            // to 4X anyhow. That just results in an aborted read attempt, with no harm done.
            List<byte> result = new List<byte>();
            Message response = null;
            bool anyRefused = false;
            while ((response = await this.device.ReceiveMessage()) != null)
            {
                this.logger.AddDebugMessage("Parsing " + response.GetBytes().ToHex());
                Protocol.HighSpeedPermissionResult parsed = this.protocol.ParseHighSpeedPermissionResponse(response);
                if (!parsed.IsValid)
                {
                    await Task.Delay(100);
                    continue;
                }

                result.Add(parsed.DeviceId);

                if (parsed.PermissionGranted)
                {
                    this.logger.AddUserMessage(string.Format("Module 0x{0:X2} ({1}) has agreed to enter high-speed mode.", parsed.DeviceId, DeviceId.DeviceCategory(parsed.DeviceId)));

                    // Forcing a notification message should help ELM devices receive responses.
                    await notifier.ForceNotify();
                    await Task.Delay(100);
                    continue;
                }

                this.logger.AddUserMessage(string.Format("Module 0x{0:X2} ({1}) has refused to enter high-speed mode.", parsed.DeviceId, DeviceId.DeviceCategory(parsed.DeviceId)));
                anyRefused = true;
            }

            if (anyRefused)
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Sends the provided message, with a retry loop. 
        /// </summary>
        public async Task<Response<bool>> WritePayload(Message message, CancellationToken cancellationToken)
        {
            int retryCount = 0;
            for (; retryCount < MaxSendAttempts; retryCount++)
            {
                await this.notifier.Notify();

                if (cancellationToken.IsCancellationRequested)
                {
                    return Response.Create(ResponseStatus.Cancelled, false, retryCount);
                }

                await Task.Delay(50); // Allow the running kernel time to enter the ReadMessage function

                if (!await device.SendMessage(message))
                {
                    this.logger.AddDebugMessage("WritePayload: Unable to send message.");
                    continue;
                }

                if (await WaitForSuccess(this.protocol.ParseUploadResponse, cancellationToken))
                {
                    return Response.Create(ResponseStatus.Success, true, retryCount);
                }

                this.logger.AddDebugMessage("WritePayload: Upload request failed.");
                await Task.Delay(100);
                await this.SendToolPresentNotification();
            }

            this.logger.AddDebugMessage("WritePayload: Giving up.");
            return Response.Create(ResponseStatus.Error, false, retryCount);
        }
    }
}
