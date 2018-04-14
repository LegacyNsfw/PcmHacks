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
            Response<Message> response1 = await this.device.SendRequest(this.messageFactory.CreateVinRequest1());
            if (response1.Status != ResponseStatus.Success)
            {
                return Response.Create(response1.Status, "Unknown");
            }

            Response<Message> response2 = await this.device.SendRequest(this.messageFactory.CreateVinRequest2());
            if (response1.Status != ResponseStatus.Success)
            {
                return Response.Create(response1.Status, "Unknown");
            }

            Response<Message> response3 = await this.device.SendRequest(this.messageFactory.CreateVinRequest3());
            if (response1.Status != ResponseStatus.Success)
            {
                return Response.Create(response1.Status, "Unknown");
            }

            return this.messageParser.ParseVinResponses(response1.Value.GetBytes(), response2.Value.GetBytes(), response3.Value.GetBytes());
        }

        /// <summary>
        /// Query the PCM's Serial Number.
        /// </summary>
        public async Task<Response<string>> QuerySerial()
        {
            Response<Message> response1 = await this.device.SendRequest(this.messageFactory.CreateSerialRequest1());
            if (response1.Status != ResponseStatus.Success)
            {
                return Response.Create(response1.Status, "Unknown");
            }

            Response<Message> response2 = await this.device.SendRequest(this.messageFactory.CreateSerialRequest2());
            if (response2.Status != ResponseStatus.Success)
            {
                return Response.Create(response2.Status, "Unknown");
            }

            Response<Message> response3 = await this.device.SendRequest(this.messageFactory.CreateSerialRequest3());
            if (response3.Status != ResponseStatus.Success)
            {
                return Response.Create(response3.Status, "Unknown");
            }

            return this.messageParser.ParseSerialResponses(response1.Value, response2.Value, response3.Value);
        }

        /// <summary>
        /// Query the PCM's Broad Cast Code.
        /// </summary>
        public async Task<Response<string>> QueryBCC()
        {
            Response<Message> response = await this.device.SendRequest(this.messageFactory.CreateBCCRequest());
            if (response.Status != ResponseStatus.Success)
            {
                return Response.Create(response.Status, "Unknown");
            }

            return this.messageParser.ParseBCCresponse(response.Value.GetBytes());
        }

        /// <summary>
        /// Query the PCM's Manufacturer Enable Counter (MEC)
        /// </summary>
        public async Task<Response<string>> QueryMEC()
        {
            Response<Message> response = await this.device.SendRequest(this.messageFactory.CreateMECRequest());
            if (response.Status != ResponseStatus.Success)
            {
                return Response.Create(response.Status, "Unknown");
            }

            return this.messageParser.ParseMECresponse(response.Value.GetBytes());
        }

        /// <summary>
        /// Update the PCM's VIN
        /// </summary>
        /// <remarks>
        /// Requires that the PCM is already unlocked
        /// </remarks>
        public async Task<Response<bool>> UpdateVin(string vin)
        {
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
            var response = await this.device.SendRequest(request);
            if (response.Status != ResponseStatus.Success)
            {
                return Response.Create(response.Status, (UInt32)0);
            }

            return this.messageParser.ParseBlockUInt32(response.Value.GetBytes());
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
            var response = await this.device.SendRequest(request);
            if (response.Status != ResponseStatus.Success)
            {
                return Response.Create(response.Status, (UInt32)0);
            }

            return this.messageParser.ParseBlockUInt32(response.Value.GetBytes());
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
            Message request = this.messageFactory.CreateCalibrationIdReadRequest();
            var response = await this.device.SendRequest(request);
            if (response.Status != ResponseStatus.Success)
            {
                return Response.Create(response.Status, (UInt32)0);
            }

            return this.messageParser.ParseBlockUInt32(response.Value.GetBytes());
        }

        /// <summary>
        /// Unlock the PCM by requesting a 'seed' and then sending the corresponding 'key' value.
        /// </summary>
        public async Task<Response<bool>> UnlockEcu()
        {
            Message seedRequest = this.messageFactory.CreateSeedRequest();
            Response<Message> seedResponse = await this.device.SendRequest(seedRequest);
            if (seedResponse.Status != ResponseStatus.Success)
            {
                if (seedResponse.Status != ResponseStatus.UnexpectedResponse) Response.Create(ResponseStatus.Success, true);
                return Response.Create(seedResponse.Status, false);
            }

            Response<UInt16> seedValueResponse = this.messageParser.ParseSeed(seedResponse.Value.GetBytes());
            if (seedValueResponse.Status != ResponseStatus.Success)
            {
                return Response.Create(seedValueResponse.Status, false);
            }

            if (seedValueResponse.Value == 0x0000)
            {
                this.logger.AddUserMessage("PCM Unlock not required");
                return Response.Create(seedValueResponse.Status, true);
            }

            UInt16 key = KeyAlgorithm.GetKey(1, seedValueResponse.Value);

            Message unlockRequest = this.messageFactory.CreateUnlockRequest(key);
            Response<Message> unlockResponse = await this.device.SendRequest(unlockRequest);
            if (unlockResponse.Status != ResponseStatus.Success)
            {
                return Response.Create(unlockResponse.Status, false);
            }

            string errorMessage;
            Response<bool> result = this.messageParser.ParseUnlockResponse(unlockResponse.Value.GetBytes(), out errorMessage);
            if (errorMessage != null)
            {
                this.logger.AddUserMessage(errorMessage);
            }
            else
            {
                this.logger.AddUserMessage("PCM Unlocked");
            }

            return result;
        }

        /// <summary>
        /// Writes a block of data to the PCM
        /// Requires an unlocked PCM
        /// </summary>
        public async Task<Response<bool>> WriteBlock(byte block, byte[] data)
        {

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
        public async Task<bool> ReadContents()
        {
            // switch to 4x, if possible. But continue either way.
            // if the vehicle bus switches but the device does not, the bus will need to time out to revert back to 1x, and the next steps will fail.
            await VehicleSetVPW4x(true);

            // execute read kernel
            Response<byte[]> response = await LoadKernelFromFile("kernel.bin");
            if (response.Status != ResponseStatus.Success) return false;

            if (!await PCMExecute(response.Value, 0xFF9150))
            {
                logger.AddUserMessage("Failed to upload kernel uploaded to PCM");
                return false;
            }

            logger.AddUserMessage("kernel uploaded to PCM succesfully");
            // read bin block by block
            // return stream
            return true;
        }

        /// <summary>
        /// Replace the full contents of the PCM.
        /// </summary>
        public Task<bool> WriteContents(Stream stream)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Load the executable payload on the PCM from the supplied address, and execute it.
        /// </summary>
        public async Task<bool> PCMExecute(byte[] payload, int address)
        {
            logger.AddDebugMessage("Going to load a " + payload.Length + " byte payload to 0x" + address.ToString("X6"));
            // Loop through the payload building and sending packets, highest first, execute on last
            for (int bytesremain = payload.Length; bytesremain > 0; bytesremain -= device.MaxSendSize)
            {
                bool exec = false;
                int length = device.MaxSendSize-12; //Headers use 10 bytes, sum uses 2 bytes
                int offset = bytesremain - length;

                if (offset<=0) // Is this the last packet?
                {
                    offset = 0;
                    length = bytesremain;
                    exec = true;
                }
                int loadaddress = address + offset;

                //if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
                //logger.AddDebugMessage("Calling CreateBlockMessage with payload size " + payload.Length + ", length " + length + " loadaddress " + loadaddress.ToString("X6") +  " exec " + exec);
                Message request = messageFactory.CreateUploadRequest(length, loadaddress);
                Response<Message> response = await SendRequest(request, 5);
                if (response.Status != ResponseStatus.Success)
                {
                    logger.AddDebugMessage("Could not upload kernel to PCM (request), aborting");
                    return false;
                }

                Message block = messageFactory.CreateBlockMessage(payload, offset, length, loadaddress, exec);
                response = await SendRequest(block, 5);
                if (response.Status != ResponseStatus.Success)
                {
                    logger.AddDebugMessage("Could not upload kernel to PCM (payload), aborting");
                    return false;
                }
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
                logger.AddUserMessage("This interface does not support VPW 4x");
                return false;
            }
            logger.AddUserMessage("This interface does support VPW 4x");

            // PCM Pre-flight checks
            Response<Message> rx = await this.device.SendRequest(HighSpeedCheck);
            if (rx.Status != ResponseStatus.Success || !Utility.CompareArraysPart(rx.Value.GetBytes(), HighSpeedOK.GetBytes()))
            {
                logger.AddUserMessage("PCM is not allowing a switch to VPW 4x");
                return false;
            }
            logger.AddUserMessage("PCM is allowing a switch to VPW 4x");

            logger.AddUserMessage("Asking PCM to swtich to VPW 4x");
            // Request all devices on the bus to change speed to VPW 4x
            rx = await this.device.SendRequest(BeginHighSpeed);

            // Request the device to change
            await device.SetVPW4x(true);

            return true;
        }

        /// <summary>
        /// Sends the provided message retries times, with a small delay on fail. 
        /// </summary>
        /// <remarks>
        /// Returns a succsefull Response on the first successful attempt, or the failed Response if we run out of tries.
        /// </remarks>
        async Task<Response<Message>> SendRequest(Message message, int retries)
        {
            Response<Message> response;
            for (int i = retries; i>0; i--)
            {
                response = await device.SendRequest(message);
                if (response.Status == ResponseStatus.Success) return response;
                await Task.Delay(10); // incase were going too fast, we might want to change this logic
            }
            return Response.Create(ResponseStatus.Error, (Message)null); // this should be response from the loop but the compiler thinks the response variable isnt in scope here????
        }
    }
}
