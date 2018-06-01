using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// From the application's perspective, this class is the API to the vehicle.
    /// </summary>
    /// <remarks>
    /// Methods in this class are high-level operations like "get the VIN," or "read the contents of the EEPROM."
    /// </remarks>
    class Vehicle : IDisposable
    {
        /// <summary>
        /// The device we'll use to talk to the PCM.
        /// </summary>
        private Device device;

        /// <summary>
        /// This class knows how to generate message to send to the PCM.
        /// </summary>
        private MessageFactory messageFactory;

        /// <summary>
        /// This class knows how to parse the messages that come in from the PCM.
        /// </summary>
        private MessageParser messageParser;

        /// <summary>
        /// This is how we send user-friendly status messages and developer-oriented debug messages to the UI.
        /// </summary>
        private ILogger logger;

        public string DeviceDescription
        {
            get
            {
                return this.device.ToString();
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Vehicle(
            Device device, 
            MessageFactory messageFactory,
            MessageParser messageParser,
            ILogger logger)
        {
            this.device = device;
            this.messageFactory = messageFactory;
            this.messageParser = messageParser;
            this.logger = logger;
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~Vehicle()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Implements IDisposable.Dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Part of the Dispose pattern.
        /// </summary>
        protected void Dispose(bool isDisposing)
        {
            if (this.device != null)
            {
                this.device.Dispose();
                this.device = null;
            }
        }

        /// <summary>
        /// Re-initialize the device.
        /// </summary>
        public async Task<bool> ResetConnection()
        {
            return await this.device.Initialize();
        }

        /// <summary>
        /// Query the PCM's VIN.
        /// </summary>
        public async Task<Response<string>> QueryVin()
        {
            this.device.ClearMessageQueue();

            if (!await this.device.SendMessage(this.messageFactory.CreateVinRequest1()))
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. Request for block 1 failed.");
            }

            Message response1 = await this.device.ReceiveMessage();
            if (response1 == null)
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. No response to request for block 1.");
            }

            if (!await this.device.SendMessage(this.messageFactory.CreateVinRequest2()))
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. Request for block 2 failed.");
            }

            Message response2 = await this.device.ReceiveMessage();
            if (response2 == null)
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. No response to request for block 2.");
            }

            if (!await this.device.SendMessage(this.messageFactory.CreateVinRequest3()))
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. Request for block 3 failed.");
            }

            Message response3 = await this.device.ReceiveMessage();
            if (response3 == null)
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. No response to request for block 3.");
            }

            return this.messageParser.ParseVinResponses(response1.GetBytes(), response2.GetBytes(), response3.GetBytes());
        }

        /// <summary>
        /// Query the PCM's Serial Number.
        /// </summary>
        public async Task<Response<string>> QuerySerial()
        {
            this.device.ClearMessageQueue();

            if (!await this.device.SendMessage(this.messageFactory.CreateSerialRequest1()))
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. Request for block 1 failed.");
            }

            Message response1 = await this.device.ReceiveMessage();
            if (response1 == null)
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. No response to request for block 1.");
            }

            if (!await this.device.SendMessage(this.messageFactory.CreateSerialRequest2()))
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. Request for block 2 failed.");
            }

            Message response2 = await this.device.ReceiveMessage();
            if (response2 == null)
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. No response to request for block 2.");
            }

            if (!await this.device.SendMessage(this.messageFactory.CreateSerialRequest3()))
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. Request for block 3 failed.");
            }

            Message response3 = await this.device.ReceiveMessage();
            if (response3 == null)
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. No response to request for block 3.");
            }

            return this.messageParser.ParseSerialResponses(response1, response2, response3);
        }

        /// <summary>
        /// Query the PCM's Broad Cast Code.
        /// </summary>
        public async Task<Response<string>> QueryBCC()
        {
            this.device.ClearMessageQueue();

            if (!await this.device.SendMessage(this.messageFactory.CreateBCCRequest()))
            {
                return Response.Create(ResponseStatus.Error, "Unknown. Request failed.");
            }

            Message response = await this.device.ReceiveMessage();
            if (response == null)
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. No response received.");
            }

            return this.messageParser.ParseBCCresponse(response.GetBytes());
        }

        /// <summary>
        /// Query the PCM's Manufacturer Enable Counter (MEC)
        /// </summary>
        public async Task<Response<string>> QueryMEC()
        {
            this.device.ClearMessageQueue();

            if (!await this.device.SendMessage(this.messageFactory.CreateMECRequest()))
            {
                return Response.Create(ResponseStatus.Error, "Unknow. Request failed.");
            }

            Message response = await this.device.ReceiveMessage();
            if (response == null)
            {
                return Response.Create(ResponseStatus.Timeout, "Unknown. No response received.");
            }

            return this.messageParser.ParseMECresponse(response.GetBytes());
        }

        /// <summary>
        /// Update the PCM's VIN
        /// </summary>
        /// <remarks>
        /// Requires that the PCM is already unlocked
        /// </remarks>
        public async Task<Response<bool>> UpdateVin(string vin)
        {
            this.device.ClearMessageQueue();

            if (vin.Length != 17) // should never happen, but....
            {
                this.logger.AddUserMessage("VIN " + vin + " is not 17 characters long!");
                return Response.Create(ResponseStatus.Error, false);
            }

            this.logger.AddUserMessage("Changing VIN to " + vin);

            byte[] bvin = Encoding.ASCII.GetBytes(vin);
            byte[] vin1 = new byte[6] { 0x00, bvin[0], bvin[1], bvin[2], bvin[3], bvin[4] };
            byte[] vin2 = new byte[6] { bvin[5], bvin[6], bvin[7], bvin[8], bvin[9], bvin[10] };
            byte[] vin3 = new byte[6] { bvin[11], bvin[12], bvin[13], bvin[14], bvin[15], bvin[16] };

            this.logger.AddUserMessage("Block 1");
            Response<bool> block1 = await WriteBlock(BlockId.Vin1, vin1);
            if (block1.Status != ResponseStatus.Success) return Response.Create(ResponseStatus.Error, false);
            this.logger.AddUserMessage("Block 2");
            Response<bool> block2 = await WriteBlock(BlockId.Vin2, vin2);
            if (block2.Status != ResponseStatus.Success) return Response.Create(ResponseStatus.Error, false);
            this.logger.AddUserMessage("Block 3");
            Response<bool> block3 = await WriteBlock(BlockId.Vin3, vin3);
            if (block3.Status != ResponseStatus.Success) return Response.Create(ResponseStatus.Error, false);

            return Response.Create(ResponseStatus.Success, true);
        }

        /// <summary>
        /// Query the PCM's operating system ID.
        /// </summary>
        /// <returns></returns>
        public async Task<Response<UInt32>> QueryOperatingSystemId()
        {
            Message request = this.messageFactory.CreateOperatingSystemIdReadRequest();
            return await this.QueryUnsignedValue(request);
        }

        /// <summary>
        /// Query the PCM's Hardware ID.
        /// </summary>
        /// <remarks>
        /// Note that this is a software variable and my not match the hardware at all of the software runs.
        /// </remarks>
        /// <returns></returns>
        public async Task<Response<UInt32>> QueryHardwareId()
        {
            Message request = this.messageFactory.CreateHardwareIdReadRequest();
            return await this.QueryUnsignedValue(request);
        }

        /// <summary>
        /// Query the PCM's Hardware ID.
        /// </summary>
        /// <remarks>
        /// Note that this is a software variable and my not match the hardware at all of the software runs.
        /// </remarks>
        /// <returns></returns>
        public async Task<Response<UInt32>> QueryCalibrationId()
        {
            //Message request = this.messageFactory.CreateCalibrationIdReadRequest();
            //return await this.QueryUnsignedValue(request);

            var query = this.CreateQuery(
                this.messageFactory.CreateCalibrationIdReadRequest,
                this.messageParser.ParseBlockUInt32);
            return await query.Execute();

        }

        private Query<T> CreateQuery<T>(Func<Message> generator, Func<Message, Response<T>> filter)
        {
            return new Query<T>(this.device, generator, filter, this.logger);
        }

        private async Task<Response<UInt32>> QueryUnsignedValue(Message request)
        {
            this.device.ClearMessageQueue();

            if (!await this.device.SendMessage(request))
            {
                return Response.Create(ResponseStatus.Error, (UInt32)0);
            }

            var response = await this.device.ReceiveMessage();
            if (response == null)
            {
                return Response.Create(ResponseStatus.Timeout, (UInt32)0);
            }

            return this.messageParser.ParseBlockUInt32(response);
        }

        private async Task SuppressChatter()
        {
            this.logger.AddDebugMessage("Suppressing VPW chatter.");
            Message suppressChatter = this.messageFactory.CreateDisableNormalMessageTransmission();
            await this.device.SendMessage(suppressChatter);
        }

        /// <summary>
        /// Unlock the PCM by requesting a 'seed' and then sending the corresponding 'key' value.
        /// </summary>
        public async Task<bool> UnlockEcu(int keyAlgorithm)
        {   
            this.device.ClearMessageQueue();

            this.logger.AddDebugMessage("Sending seed request.");
            Message seedRequest = this.messageFactory.CreateSeedRequest();

            if (!await this.device.SendMessage(seedRequest))
            {
                this.logger.AddDebugMessage("Unable to send seed request.");
                return false;
            }

            Message seedResponse = await this.device.ReceiveMessage();
            if (seedResponse == null)
            {
                this.logger.AddDebugMessage("No response to seed request.");
                return false;
            }

            if (this.messageParser.IsUnlocked(seedResponse.GetBytes()))
            {
                this.logger.AddUserMessage("PCM is already unlocked");
                return true;
            }

            this.logger.AddDebugMessage("Parsing seed value.");
            Response<UInt16> seedValueResponse = this.messageParser.ParseSeed(seedResponse.GetBytes());
            if (seedValueResponse.Status != ResponseStatus.Success)
            {
                this.logger.AddDebugMessage("Unable to parse seed response.");
                return false;
            }

            if (seedValueResponse.Value == 0x0000)
            {
                this.logger.AddUserMessage("PCM Unlock not required");
                return true;
            }

            UInt16 key = KeyAlgorithm.GetKey(keyAlgorithm, seedValueResponse.Value);

            this.logger.AddDebugMessage("Sending unlock request (" + seedValueResponse.Value.ToString("X4") + ", " + key.ToString("X4") + ")");
            Message unlockRequest = this.messageFactory.CreateUnlockRequest(key);
            if (!await this.device.SendMessage(unlockRequest))
            {
                this.logger.AddDebugMessage("Unable to send unlock request.");
                return false;
            }

            Message unlockResponse = await this.device.ReceiveMessage();
            if (unlockResponse == null)
            {
                this.logger.AddDebugMessage("No response to unlock request.");
                return false;
            }
            
            string errorMessage;
            Response<bool> result = this.messageParser.ParseUnlockResponse(unlockResponse.GetBytes(), out errorMessage);
            if (errorMessage != null)
            {
                this.logger.AddUserMessage(errorMessage);
            }

            return result.Value;
        }

        /// <summary>
        /// Writes a block of data to the PCM
        /// Requires an unlocked PCM
        /// </summary>
        private async Task<Response<bool>> WriteBlock(byte block, byte[] data)
        {
            /*
            Message tx;
            Message ok = new Message(new byte[] { 0x6C, DeviceId.Tool, DeviceId.Pcm, 0x7B, block });

            switch (data.Length)
            {
                case 6:
                    tx = new Message(new byte[] { 0x6C, DeviceId.Pcm, DeviceId.Tool, 0x3B, block, data[0], data[1], data[2], data[3], data[4], data[5] });
                    break;
                default:
                    logger.AddDebugMessage("Cant write block size " + data.Length);
                    return Response.Create(ResponseStatus.Error, false);
            }

            Response<Message> rx = await this.device.SendRequest(tx);

            if (rx.Status != ResponseStatus.Success)
            {
                logger.AddUserMessage("Failed to write block " + block + ", communications failure");
                return Response.Create(ResponseStatus.Error, false);
            }

            if (!Utility.CompareArrays(rx.Value.GetBytes(), ok.GetBytes()))
            {
                logger.AddUserMessage("Failed to write block " + block + ", PCM rejected attempt");
                return Response.Create(ResponseStatus.Error, false);
            }

            logger.AddDebugMessage("Successful write to block " + block);
            return Response.Create(ResponseStatus.Success, true);
            */
            return Response.Create(ResponseStatus.Error, false);
        }

        public async Task<byte[]> LoadKernelFromFidle(string kernel)
        {
            using (Stream stream = File.OpenRead(kernel))
            {
                byte[] contents = new byte[stream.Length];
                await stream.ReadAsync(contents, 0, (int)stream.Length);
                return contents;
            }
        }

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
        /// Read the full contents of the PCM.
        /// Assumes the PCM is unlocked an were ready to go
        /// </summary>
        public async Task<Response<Stream>> ReadContents(PcmInfo info)
        {
            try
            {
                this.device.ClearMessageQueue();

                // switch to 4x, if possible. But continue either way.
                // if the vehicle bus switches but the device does not, the bus will need to time out to revert back to 1x, and the next steps will fail.
                await this.VehicleSetVPW4x(true);

                // execute read kernel
                Response<byte[]> response = await LoadKernelFromFile("kernel.bin");
                if (response.Status != ResponseStatus.Success)
                {
                    logger.AddUserMessage("Failed to load kernel from file.");
                    return new Response<Stream>(response.Status, null);
                }

                ToolPresentNotifier toolPresentNotifier = new ToolPresentNotifier(this.logger, this.messageFactory, this.device);

                await toolPresentNotifier.Notify();

                // TODO: instead of this hard-coded 0xFF9150, get the base address from the PcmInfo object.
                if (!await PCMExecute(response.Value, 0xFF9150))
                {
                    logger.AddUserMessage("Failed to upload kernel uploaded to PCM");
                    return new Response<Stream>(ResponseStatus.Error, null);
                }

                logger.AddUserMessage("kernel uploaded to PCM succesfully");

                int startAddress = info.ImageBaseAddress;
                int endAddress = info.ImageBaseAddress + info.ImageSize;
                int bytesRemaining = info.ImageSize;
                int blockSize = this.device.MaxReceiveSize - 10 - 2; // allow space for the header and block checksum

                byte[] image = new byte[info.ImageSize];

                while (startAddress < endAddress)
                {
                    await toolPresentNotifier.Notify();

                    if (startAddress + blockSize > endAddress)
                    {
                        blockSize = endAddress - startAddress;
                    }

                    if (blockSize < 1)
                    {
                        this.logger.AddUserMessage("Image download complete");
                        break;
                    }
                    
                    if (!await TryReadBlock(image, startAddress, blockSize))
                    {
                        this.logger.AddUserMessage(
                            string.Format(
                                "Unable to read block from {0} to {1}",
                                startAddress,
                                blockSize));
                        return new Response<Stream>(ResponseStatus.Error, null);
                    }

                    startAddress += blockSize;
                }

                MemoryStream stream = new MemoryStream(image);
                return new Response<Stream>(ResponseStatus.Success, stream);
            }
            catch(Exception exception)
            {
                this.logger.AddUserMessage("Something went wrong. " + exception.Message);
                this.logger.AddDebugMessage(exception.ToString());
                return new Response<Stream>(ResponseStatus.Error, null);
            }
            finally
            {
                // Sending the exit command twice, and at both speeds, just to
                // be totally certain that the PCM goes back to normal. If the
                // kernel is left running, the engine won't start, and the 
                // dashboard lights up with all sorts of errors.
                //
                // You can reset by pulling the PCM's fuse, but I'd hate to 
                // have a user think that we've done some real damage before 
                // they figure that out.
                await this.ExitKernel();
                await this.VehicleSetVPW4x(false);
                await this.ExitKernel();
            }
        }

        public async Task ExitKernel()
        {
            this.device.ClearMessageQueue();

            Message exitKernel = this.messageFactory.CreateExitKernel();
            await this.device.SendMessage(exitKernel);
        }

        private async Task<bool> TryReadBlock(byte[] image, int startAddress, int length)
        {
            this.logger.AddDebugMessage(string.Format("Reading from {0}, length {1}", startAddress, length));
            
            for(int sendAttempt = 1; sendAttempt <= 5; sendAttempt++)
            {
                Message message = this.messageFactory.CreateReadRequest(startAddress, length);

                this.logger.AddDebugMessage("Sending " + message.GetBytes().ToHex());
                if (!await this.device.SendMessage(message))
                {
                    this.logger.AddDebugMessage("Unable to send read request.");
                    return false;
                }

                bool sendAgain = false;
                for (int receiveAttempt = 1; receiveAttempt <= 5; receiveAttempt++)
                {
                    Message response = await this.ReceiveMessage();
                    if (response == null)
                    {
                        this.logger.AddDebugMessage("Did not receive a response to the read request.");
                        sendAgain = true;
                        break;
                    }

                    this.logger.AddDebugMessage("Processing message " + response.GetBytes().ToHex());

                    Response<bool> readResponse = this.messageParser.ParseReadResponse(response);
                    if (readResponse.Status != ResponseStatus.Success)
                    {
                        this.logger.AddDebugMessage("Not a read response.");
                        continue;
                    }

                    if (!readResponse.Value)
                    {
                        this.logger.AddDebugMessage("Read request failed.");
                        sendAgain = true;
                        break;
                    }

                    // We got a successful read response, so now wait for the payload.
                    sendAgain = false;
                    break;
                }

                if (sendAgain)
                {
                    continue;
                }

                this.logger.AddDebugMessage("Read request allowed, expecting for payload...");
                for (int receiveAttempt = 1; receiveAttempt <= 5; receiveAttempt++)
                {   
                    Message payloadMessage = await this.device.ReceiveMessage();
                    if (payloadMessage == null)
                    {
                        this.logger.AddDebugMessage("No payload following read request.");
                        continue;
                    }

                    this.logger.AddDebugMessage("Processing message " + payloadMessage.GetBytes().ToHex());

                    Response<byte[]> payloadResponse = this.messageParser.ParsePayload(payloadMessage);
                    if (payloadResponse.Status != ResponseStatus.Success)
                    {
                        this.logger.AddDebugMessage("Not payload message.");
                        continue;
                    }

                    byte[] payload = payloadResponse.Value;
                    Buffer.BlockCopy(payload, 0, image, startAddress, length);

                    int percentDone = (startAddress * 100) / image.Length;
                    this.logger.AddUserMessage(string.Format("Recieved block starting at {0} / 0x{0:X}. {1}%", startAddress, percentDone));

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Wait for an incoming message.
        /// </summary>
        private async Task<Message> ReceiveMessage()
        {
            Message response = null;

            for (int pause = 0; pause < 10; pause++)
            {
                response = await this.device.ReceiveMessage();
                if (response == null)
                {
                    this.logger.AddDebugMessage("No response to read request yet.");
                    await Task.Delay(10);
                    continue;
                }

                break;
            }

            return response;
        }

        /// <summary>
        /// Replace the full contents of the PCM.
        /// </summary>
        public Task<bool> WriteContents(Stream stream)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Read messages from the device, ignoring irrelevant messages.
        /// </summary>
        private async Task<bool> WaitForSuccess(Func<Message, Response<bool>> filter)
        {
            for(int attempt = 1; attempt<=5; attempt++)
            {
                Message message = await this.device.ReceiveMessage();
                if(message == null)
                {
                    continue;
                }

                Response<bool> response = filter(message);
                if (response.Status != ResponseStatus.Success)
                {
                    this.logger.AddDebugMessage("Ignoring unrelated message.");
                    continue;
                }

                this.logger.AddDebugMessage("Found response, " + (response.Value ? "succeeded." : "failed."));
                return response.Value;
            }

            return false;
        }

        /// <summary>
        /// Load the executable payload on the PCM at the supplied address, and execute it.
        /// </summary>
        public async Task<bool> PCMExecute(byte[] payload, int address)
        {
            await this.SuppressChatter();

            logger.AddDebugMessage("Sending upload request with payload size " + payload.Length + ", loadaddress " + address.ToString("X6"));
            Message request = messageFactory.CreateUploadRequest(payload.Length, address);
            if(!await this.device.SendMessage(request))
            {
                logger.AddUserMessage("Unable to send request to upload kernel to RAM.");
                return false;
            }

            if (!await this.WaitForSuccess(this.messageParser.ParseUploadPermissionResponse))
            {
                logger.AddUserMessage("Permission to upload kernel was denied.");
                return false;
            }
            
            logger.AddDebugMessage("Going to load a " + payload.Length + " byte payload to 0x" + address.ToString("X6"));

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

            Response<bool> uploadResponse = await WriteToRam(remainderMessage, 5);
            if (uploadResponse.Status != ResponseStatus.Success)
            {
                logger.AddDebugMessage("Could not upload kernel to PCM, remainder payload not accepted.");
                return false;
            }

            // Now we send a series of full upload packets
            for (int chunkIndex = chunkCount; chunkIndex > 0; chunkIndex--)
            {
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

                uploadResponse = await WriteToRam(payloadMessage, 5);
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
        public async Task<bool> VehicleSetVPW4x(bool highspeed)
        {
            Message HighSpeedCheck = messageFactory.CreateHighSpeedCheck();
            Message HighSpeedOK = messageFactory.CreateHighSpeedOKResponse();
            Message BeginHighSpeed = messageFactory.CreateBeginHighSpeed();

            if (!device.Supports4X) 
            {
                if (highspeed)
                {
                    // where there is no support only report no switch to 4x
                    logger.AddUserMessage("This interface does not support VPW 4x");
                }
                return true;
            }
            
            // Configure the vehicle bus when switching to 4x
            if (highspeed)
            {
                logger.AddUserMessage("Attempting switch to VPW 4x");
                // PCM Pre-flight checks
                if (!await this.device.SendMessage(HighSpeedCheck))
                {
                    logger.AddUserMessage("Unable to request permission to use 4x.");
                    return false;
                }

                Message rx = await this.device.ReceiveMessage();
                if (rx == null)
                {
                    logger.AddUserMessage("No response received to high-speed permission request.");
                    return false;
                }

                if (!Utility.CompareArraysPart(rx.GetBytes(), HighSpeedOK.GetBytes()))
                {
                    logger.AddUserMessage("PCM is not allowing a switch to VPW 4x");
                    return false;
                }

                logger.AddUserMessage("PCM is allowing a switch to VPW 4x. Requesting all VPW modules to do so.");
                if (!await this.device.SendMessage(BeginHighSpeed))
                {
                    return false;
                }
            }
            else
            {
                logger.AddUserMessage("Reverting to VPW 1x");
            }

            // Request the device to change
            await device.SetVPW4x(highspeed);

            return true;
        }

        /// <summary>
        /// Sends the provided message retries times, with a small delay on fail. 
        /// </summary>
        /// <remarks>
        /// Returns a succsefull Response on the first successful attempt, or the failed Response if we run out of tries.
        /// </remarks>
        async Task<Response<bool>> WriteToRam(Message message, int retries)
        {
            for (int i = retries; i>0; i--)
            {
                if (!await device.SendMessage(message))
                {
                    this.logger.AddDebugMessage("WriteToRam: Unable to send message.");
                    continue;
                }

                if (await this.WaitForSuccess(this.messageParser.ParseUploadResponse))
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
