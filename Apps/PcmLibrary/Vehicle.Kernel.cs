﻿using System;
using System.Collections.Generic;
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
            Message suppressChatter = this.messageFactory.CreateDisableNormalMessageTransmission();
            await this.device.SendMessage(suppressChatter);
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

            if (path == "") return Response.Create(ResponseStatus.Error, file);

            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string exeDirectory = Path.GetDirectoryName(exePath);
            path = Path.Combine(exeDirectory, path);

            try
            {
                using (Stream fileStream = File.OpenRead(path))
                {
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
            this.logger.AddDebugMessage("Cleaning up Flash Kernel");
            await this.ExitKernel();
            this.logger.AddDebugMessage("Clear DTCs");
            await this.ClearDTCs();
        }

        /// <summary>
        /// Exits the kernel at 4x, then at 1x. Once this function has been called the bus will be back at 1x.
        /// </summary>
        /// <remarks>
        /// Can be used to force exit the kernel, if requied. Does not attempt the 4x exit if not supported by the current device.
        /// </remarks>
        public async Task ExitKernel()
        {
            Message exitKernel = this.messageFactory.CreateExitKernel();

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
        /// Clears DTCs
        /// </summary>
        /// <remarks>
        /// Return code is not checked as its an uncommon mode and IDs, different devices will handle this differently.
        /// </remarks>
        public async Task ClearDTCs()
        {
            Message ClearDTCs = this.messageFactory.CreateClearDTCs();
            Message ClearDTCsOK = this.messageFactory.CreateClearDTCsOK();

            await this.device.SendMessage(ClearDTCs);
            this.device.ClearMessageQueue();
        }

        /// <summary>
        /// Load the executable payload on the PCM at the supplied address, and execute it.
        /// </summary>
        public async Task<bool> PCMExecute(byte[] payload, int address, CancellationToken cancellationToken)
        {
            logger.AddUserMessage("Uploading kernel to PCM.");

            logger.AddDebugMessage("Sending upload request with payload size " + payload.Length + ", loadaddress " + address.ToString("X6"));
            Message request = messageFactory.CreateUploadRequest(payload.Length, address);

            if(!await TrySendMessage(request, "upload request"))
            {
                return false;
            }

            if (!await this.WaitForSuccess(this.messageParser.ParseUploadPermissionResponse, cancellationToken))
            {
                logger.AddUserMessage("Permission to upload kernel was denied.");
                return false;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            logger.AddDebugMessage("Going to load a " + payload.Length + " byte payload to 0x" + address.ToString("X6"));

            await this.device.SetTimeout(TimeoutScenario.SendKernel);

            // Loop through the payload building and sending packets, highest first, execute on last
            int payloadSize = device.MaxSendSize - 12; // Headers use 10 bytes, sum uses 2 bytes.
            int chunkCount = payload.Length / payloadSize;
            int remainder = payload.Length % payloadSize;

            int offset = (chunkCount * payloadSize);
            int startAddress = address + offset;

            // First we send the 'remainder' payload, containing any bytes that won't fill up an entire upload packet.
            logger.AddDebugMessage(
                string.Format(
                    "Sending remainder payload with offset 0x{0:X}, start address 0x{1:X}, length 0x{2:X}.",
                    offset,
                    startAddress,
                    remainder));

            Message remainderMessage = messageFactory.CreateBlockMessage(
                payload, 
                offset, 
                remainder, 
                address + offset, 
                remainder == payload.Length);

            Response<bool> uploadResponse = await WriteToRam(remainderMessage, cancellationToken);
            if (uploadResponse.Status != ResponseStatus.Success)
            {
                logger.AddDebugMessage("Could not upload kernel to PCM, remainder payload not accepted.");
                return false;
            }

            // Now we send a series of full upload packets
            for (int chunkIndex = chunkCount; chunkIndex > 0; chunkIndex--)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                offset = (chunkIndex - 1) * payloadSize;
                startAddress = address + offset;
                Message payloadMessage = messageFactory.CreateBlockMessage(
                    payload,
                    offset,
                    payloadSize,
                    startAddress,
                    offset == 0);

                logger.AddDebugMessage(
                    string.Format(
                        "Sending payload with offset 0x{0:X}, start address 0x{1:X}, length 0x{2:X}.",
                        offset,
                        startAddress,
                        payloadSize));

                uploadResponse = await WriteToRam(payloadMessage, cancellationToken);
                if (uploadResponse.Status != ResponseStatus.Success)
                {
                    logger.AddDebugMessage("Could not upload kernel to PCM, payload not accepted.");
                    return false;
                }

                int bytesSent = payload.Length - offset;
                int percentDone = bytesSent * 100 / payload.Length;

                this.logger.AddUserMessage(
                    string.Format(
                        "Kernel upload {0}% complete.",
                        percentDone));
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
            
            // Configure the vehicle bus when switching to 4x
            if (newSpeed == VpwSpeed.FourX)
            {
                logger.AddUserMessage("Attempting switch to VPW 4x");
                await device.SetTimeout(TimeoutScenario.ReadProperty);

                // The list of modules may not be useful after all, but 
                // checking for an empty list indicates an uncooperative
                // module on the VPW bus.
                List<byte> modules = await this.RequestHighSpeedPermission();
                if (modules == null)
                {
                    // A device has refused the switch to high speed mode.
                    return false;
                }

                Message broadcast = this.messageFactory.CreateBeginHighSpeed(DeviceId.Broadcast);
                await this.device.SendMessage(broadcast);

                // Check for any devices that refused to switch to 4X speed.
                // These responses usually get lost, so this code might be pointless.
                Message response = null;
                while ((response = await this.device.ReceiveMessage()) != null)
                {
                    Response<bool> refused = this.messageParser.ParseHighSpeedRefusal(response);
                    if (refused.Status != ResponseStatus.Success)
                    {
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

            TimeoutScenario scenario = newSpeed == VpwSpeed.Standard ? TimeoutScenario.ReadProperty : TimeoutScenario.ReadMemoryBlock;
            await device.SetTimeout(scenario);

            return true;
        }
        
        /// <summary>
        /// Ask all of the devices on the VPW bus for permission to switch to 4X speed.
        /// </summary>
        private async Task<List<byte>> RequestHighSpeedPermission()
        {
            Message permissionCheck = this.messageFactory.CreateHighSpeedPermissionRequest(DeviceId.Broadcast);
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
                MessageParser.HighSpeedPermissionResult parsed = this.messageParser.ParseHighSpeedPermissionResponse(response);
                if (!parsed.IsValid)
                {
                    continue;
                }

                result.Add(parsed.DeviceId);

                if (parsed.PermissionGranted)
                {
                    this.logger.AddUserMessage(string.Format("Module 0x{0:X2} ({1}) has agreed to enter high-speed mode.", parsed.DeviceId, DeviceId.DeviceCategory(parsed.DeviceId)));
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
        /// Sends the provided message retries times, with a small delay on fail. 
        /// </summary>
        /// <remarks>
        /// Returns a succsefull Response on the first successful attempt, or the failed Response if we run out of tries.
        /// </remarks>
        async Task<Response<bool>> WriteToRam(Message message, CancellationToken cancellationToken)
        {
            for (int i = MaxSendAttempts; i>0; i--)
            {
                if (!await device.SendMessage(message))
                {
                    this.logger.AddDebugMessage("WriteToRam: Unable to send message.");
                    continue;
                }

                if (await this.WaitForSuccess(this.messageParser.ParseUploadResponse, cancellationToken))
                {
                    return Response.Create(ResponseStatus.Success, true);
                }

                this.logger.AddDebugMessage("WriteToRam: Upload request failed.");
            }

            this.logger.AddDebugMessage("WriteToRam: Giving up.");
            return Response.Create(ResponseStatus.Error, false); // this should be response from the loop but the compiler thinks the response variable isnt in scope here????
        }
    }
}