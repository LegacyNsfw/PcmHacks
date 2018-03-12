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
            return "ScanTool OBDLink MX or SX";
        }

        public override async Task<bool> Initialize()
        {
            this.Logger.AddDebugMessage("Initializing " + this.ToString());

            SerialPortConfiguration configuration = new SerialPortConfiguration();
            configuration.BaudRate = 115200;
            await this.Port.OpenAsync(configuration);

            // Turn off echo.
            this.Logger.AddDebugMessage(await this.SendRequest("ATE0"));

            string elmId = await this.SendRequest("AT I");
            this.Logger.AddUserMessage("Device supports " + elmId);

            string stId = await this.SendRequest("ST I");
            this.Logger.AddUserMessage("Device supports " + stId);

            string voltage = await this.SendRequest("AT RV");
            this.Logger.AddUserMessage("Voltage: " + voltage);

            const string prompt = "\r\r>";
            if (!await this.SendAndVerify("AT AL", "OK" + prompt) ||
                !await this.SendAndVerify("AT SP2", "OK" + prompt) ||
                !await this.SendAndVerify("AT DP", "SAE J1850 VPW" + prompt))
            {
                throw new Exception("Could not initalize " + this.ToString());
            }

            return true;
        }

        private async Task<bool> SendAndVerify(string message, string expectedResponse)
        {
            string actualResponse = await this.SendRequest(message);
            
            if (string.Equals(actualResponse, expectedResponse))
            {
                this.Logger.AddDebugMessage("Good." + Environment.NewLine);
                return true;
            }

            this.Logger.AddDebugMessage("Bad. Expected " + expectedResponse);
            return false;
        }

        private async Task<string> SendRequest(string request)
        {
            this.Logger.AddDebugMessage("Sending " + request);
            await this.Port.Send(Encoding.ASCII.GetBytes(request + "\r\n"));

            // This is to make sure the whole response is available.
            await Task.Delay(100);

            byte[] responseBytes = new byte[100];
            int length = await this.Port.Receive(responseBytes, 0, 100);
            string response = Encoding.ASCII.GetString(responseBytes, 0, length);
            this.Logger.AddDebugMessage("Received " + response);
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
        public async override Task<Response<byte[]>> SendRequest(Message message)
        {
            byte[] bytes = message.GetBytes();
            byte crc = Utility.ComputeCrc(bytes);
            byte[] bytesWithCrc = new byte[bytes.Length + 1];
            bytes.CopyTo(bytesWithCrc, 0);
            bytesWithCrc[bytes.Length] = crc;

            string hexRequest = bytesWithCrc.ToHex(); // bytes.ToHex(); // 

            try
            {
                string hexResponse = await this.SendRequest(hexRequest);
                
                if (!hexResponse.IsHex())
                {
                    this.Logger.AddDebugMessage("Unexpected response: " + hexResponse);
                    Response<byte[]> result = new Response<byte[]>(ResponseStatus.UnexpectedResponse, new byte[0]);
                    return result;
                }

                byte[] value = hexResponse.ToBytes();
                return new Response<byte[]>(ResponseStatus.Success, value);
            }
            catch (TimeoutException)
            {
                Response<byte[]> result = new Response<byte[]>(ResponseStatus.Timeout, new byte[0]);
                return result;
            }
        }
    }
}
