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
        /// Query the PCM's VIN.
        /// </summary>
        public async Task<Response<string>> QueryVin()
        {
            Response<byte[]> response1 = await this.device.SendRequest(this.messageFactory.CreateVinRequest1());
            if (response1.Status != ResponseStatus.Success)
            {
                return Response.Create(response1.Status, "Unknown");
            }

            Response<byte[]> response2 = await this.device.SendRequest(this.messageFactory.CreateVinRequest2());
            if (response1.Status != ResponseStatus.Success)
            {
                return Response.Create(response1.Status, "Unknown");
            }

            Response<byte[]> response3 = await this.device.SendRequest(this.messageFactory.CreateVinRequest3());
            if (response1.Status != ResponseStatus.Success)
            {
                return Response.Create(response1.Status, "Unknown");
            }

            return this.messageParser.ParseVinResponses(response1, response2, response3);
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

            return this.messageParser.ParseOperatingSystemId(response);
        }

        /// <summary>
        /// Unlock the PCM by requesting a 'seed' and then sending the corresponding 'key' value.
        /// </summary>
        public async Task<Response<bool>> UnlockEcu()
        {
            Message seedRequest = this.messageFactory.CreateSeedRequest();
            var seedResponse = await this.device.SendRequest(seedRequest);
            if (seedResponse.Status != ResponseStatus.Success)
            {
                return Response.Create(seedResponse.Status, false);
            }

            Response<UInt16> seedValueResponse = this.messageParser.ParseSeed(seedResponse);
            if (seedValueResponse.Status != ResponseStatus.Success)
            {
                return Response.Create(seedValueResponse.Status, false);
            }

            UInt16 key = KeyAlgorithm.GetKey(seedValueResponse.Value);

            Message unlockRequest = this.messageFactory.CreateUnlockRequest(key);
            var unlockResponse = await this.device.SendRequest(unlockRequest);
            if (unlockResponse.Status != ResponseStatus.Success)
            {
                return Response.Create(unlockResponse.Status, false);
            }

            string errorMessage;
            Response<bool> result = this.messageParser.ParseUnlockResponse(unlockResponse, out errorMessage);
            if (errorMessage != null)
            {
                this.logger.AddUserMessage(errorMessage);
            }

            return result;
        }

        /// <summary>
        /// Read the full contents of the PCM.
        /// </summary>
        public Task<Response<Stream>> ReadContents()
        {
            return Task.FromResult(new Response<Stream>(ResponseStatus.Success, (Stream)new MemoryStream(new byte[] { 0x01, 0x02, 0x03 })));
        }

        /// <summary>
        /// Replace the full contents of the PCM.
        /// </summary>
        public Task<bool> WriteContents(Stream stream)
        {
            return Task.FromResult(true);
        }
    }
}
