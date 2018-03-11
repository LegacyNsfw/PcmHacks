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
        private Device device;
        private MessageFactory messageFactory;
        private MessageParser messageParser;
        private ILogger logger;

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

        ~Vehicle()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected void Dispose(bool isDisposing)
        {
            this.device.Dispose();
        }

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

            return Response.Create(ResponseStatus.Success, true);
        }

        public Task<Stream> ReadContents()
        {
            // TODO: actually read from the ECU.
            return Task.FromResult((Stream)new MemoryStream(new byte[] { 0x01, 0x02, 0x03 }));
        }

        public Task<bool> WriteContents(Stream stream)
        {
            return Task.FromResult(true);
        }
    }
}
