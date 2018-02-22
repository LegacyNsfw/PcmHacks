using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// This class provides a way to test most of the app without any interface hardware.
    /// </summary>
    class MockInterface : Interface
    {
        public MockInterface(IPort port) : base(port)
        {

        }

        public override string ToString()
        {
            return "Mock Interface";
        }

        public override async Task<string> QueryVin()
        {
            /*
            byte[] message = Protocol.CreateVinQuery();
            await this.Port.Send(message);

            byte[] buffer = new byte[1000];
            int bytesRead = await this.Port.Receive(buffer, 0, buffer.Length);

            string vin = Protocol.ExtractVin(buffer);
            */
            return await Task.FromResult("Mock VIN");
        }

        public override async Task<string> QueryOS()
        {
            return await Task.FromResult("Mock OS");
        }

        public override async Task<int> QuerySeed()
        {
            return await Task.FromResult(0x1234);
        }

        public override async Task<bool> SendKey(int key)
        {
            return await Task.FromResult(true);
        }

        public override async Task<bool> SendKernel()
        {
            return await Task.FromResult(true);
        }

        public override Task<Stream> ReadContents()
        {
            return Task.FromResult((Stream)new MemoryStream());
        }
    }
}
