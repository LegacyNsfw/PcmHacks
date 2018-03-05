using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    class Vehicle : IDisposable
    {
        private Device device;
        private MessageFactory messageFactory;
        private MessageParser messageParser;

        public Vehicle(
            Device device, 
            MessageFactory messageFactory,
            MessageParser messageParser)
        {
            this.device = device;
            this.messageFactory = messageFactory;
            this.messageParser = messageParser;
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

            UInt16 seed = this.messageParser.ParseSeed(seedResponse);
            UInt16 key = KeyAlgorithm.GetKey(seed);

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
            return null;
        }
    }
}
