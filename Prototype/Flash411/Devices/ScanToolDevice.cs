using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// This class encapsulates all code that is unique to the ScanTool MX interface.
    /// </summary>
    class ScanToolDevice : Device
    {
        public ScanToolDevice(IPort port, ILogger logger) : base(port, logger)
        {

        }

        public override string ToString()
        {
            return "ScanTool MX or SX";
        }

        public override async Task<bool> Initialize()
        {
            SerialPortConfiguration configuration = new SerialPortConfiguration();
            configuration.BaudRate = 38400;
            await this.Port.OpenAsync(configuration);

            await this.Port.Send(Encoding.ASCII.GetBytes("ATDI"));
            string deviceId = await ReceiveString();

            await this.Port.Send(Encoding.ASCII.GetBytes("ATI"));
            string firmware = await ReceiveString();

            this.Logger.AddUserMessage("Connected to " + deviceId);
            this.Logger.AddUserMessage("Firmware " + firmware);

            return true;
        }

        private async Task<string> ReceiveString()
        {
            byte[] responseBytes = new byte[100];
            await this.Port.Receive(responseBytes, 0, 100);
            string response = Encoding.ASCII.GetString(responseBytes);
            return response;
        }

        /// <summary>
        /// Send a message, do not expect a response.
        /// </summary>
        public override Task<bool> SendMessage(Message message)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Send a message, wait for a response, return the response.
        /// </summary>
        public override Task<Response<byte[]>> SendRequest(Message message)
        {
            return Task.FromResult(Response.Create(ResponseStatus.Success, new byte[] { }));
        }
    }
}
