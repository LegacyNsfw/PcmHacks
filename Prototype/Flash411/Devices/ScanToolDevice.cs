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
        /// <summary>
        /// Every response from an ELM device ends with this.
        /// </summary>
        private const string Prompt = "\r\r>";

        /// <summary>
        /// Constructor.
        /// </summary>
        public ScanToolDevice(IPort port, ILogger logger) : base(port, logger)
        {

        }

        /// <summary>
        /// This string is what will appear in the drop-down list in the UI.
        /// </summary>
        public override string ToString()
        {
            return "ScanTool OBDLink MX or SX";
        }

        /// <summary>
        /// Configure the device for use - and also confirm that the device is what we think it is.
        /// </summary>
        public override async Task<bool> Initialize()
        {
            this.Logger.AddDebugMessage("Initializing " + this.ToString());

            SerialPortConfiguration configuration = new SerialPortConfiguration();
            configuration.BaudRate = 115200;
            await this.Port.OpenAsync(configuration);

            await this.Port.DiscardBuffers();

            try
            {
                // Reset
                // this.Logger.AddDebugMessage(await this.SendRequest("AT Z"));

                // Turn off echo.
                this.Logger.AddDebugMessage(await this.SendRequest("AT E0"));

                // TODO: Turn off whitespace.
                // this.Logger.AddDebugMessage(await this.SendRequest("AT S0"));

                string elmId = await this.SendRequest("AT I");
                this.Logger.AddUserMessage("Device supports " + elmId);

                string stId = await this.SendRequest("ST I");
                this.Logger.AddUserMessage("Device supports " + stId);

                string voltage = await this.SendRequest("AT RV");
                this.Logger.AddUserMessage("Voltage: " + voltage);

                if (!await this.SendAndVerify("AT AL", "OK") ||
                    !await this.SendAndVerify("AT SP2", "OK") ||
                    !await this.SendAndVerify("AT DP", "SAE J1850 VPW"))
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                this.Logger.AddDebugMessage("Unable to initalize " + this.ToString());
                this.Logger.AddDebugMessage(exception.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Send a message, do not expect a response.
        /// </summary>
        public override Task<bool> SendMessage(Message message)
        {
            // Not yet implemented.
            return Task.FromResult(true);
        }

        /// <summary>
        /// Send a message, wait for a response, return the response.
        /// </summary>
        /// <remarks>
        /// The 'message' parameter contains the exact sequence of bytes to send to the PCM,
        /// and this method must return the exact sequence of bytes that came from the PCM.
        /// The ELM/STN protocol gets in the way of that, so we have to translate in both directions.
        /// </remarks>
        public async override Task<Response<Message>> SendRequest(Message message)
        {
            // The incoming byte array needs to separated into header and payload portions,
            // which are sent separately.
            //
            // This may not be necessary after converting to the STN extensions to the ELM protocol. We'll see.
            byte[] messageBytes = message.GetBytes();
            string hexRequest = messageBytes.ToHex();
            string header = hexRequest.Substring(0, 9);

            string setHeaderResponse = await this.SendRequest("AT SH " + header);
            if (setHeaderResponse != "OK")
            {
                this.Logger.AddDebugMessage("Unexpected response to set-header command: " + setHeaderResponse);
                return Response.Create(ResponseStatus.UnexpectedResponse, (Message) null);
            }

            string payload = hexRequest.Substring(9);

            try
            {
                string hexResponse = await this.SendRequest(payload);

                // Make sure we can parse the response.
                if (!hexResponse.IsHex())
                {
                    this.Logger.AddDebugMessage("Unexpected response: " + hexResponse);
                    return Response.Create(ResponseStatus.UnexpectedResponse, (Message)null);
                }

                // Add the header bytes that the ELM hides from us.
                byte[] deviceResponseBytes = hexResponse.ToBytes();
                byte[] completeResponseBytes = new byte[deviceResponseBytes.Length + 3];
                completeResponseBytes[0] = messageBytes[0];
                completeResponseBytes[1] = messageBytes[2];
                completeResponseBytes[2] = messageBytes[1];
                deviceResponseBytes.CopyTo(completeResponseBytes, 3);

                return Response.Create(ResponseStatus.Success, new Message(completeResponseBytes));
            }
            catch (TimeoutException)
            {
                return Response.Create(ResponseStatus.Timeout, (Message)null);
            }
        }

        /// <summary>
        /// Send a command to the device, confirm that we got the response we expect. 
        /// </summary>
        /// <remarks>
        /// This is primarily for use in the Initialize method, where we're talking to the 
        /// interface device rather than the PCM.
        /// </remarks>
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

        /// <summary>
        /// Send a request, wait for a response.
        /// </summary>
        /// <remarks>
        /// The API for this method (sending a string, returning a string) matches
        /// the way that we need to communicate with ELM and STN devices.
        /// </remarks>
        private async Task<string> SendRequest(string request)
        {
            this.Logger.AddDebugMessage("Sending " + request);
            await this.Port.Send(Encoding.ASCII.GetBytes(request + "\r\n"));

            // I hate this, but it improves reliability...
            await Task.Delay(250);

            List<byte> responseBytes = new List<byte>(100);
            int pauseCount = 0;

            // This 'for' loop just sets an upper limit on the number of times we'll retry,
            // to ensure that we won't just loop indefinitely if something goes wrong.
            //
            // In practice, the loop should always end when the "\r\r>" marker is received.
            for(int iterations = 0; iterations < 1000; iterations++)
            {
                byte[] buffer = new byte[100];
                int length = await this.Port.Receive(buffer, 0, buffer.Length);

                if (length == 0) 
                {
                    // When no data is received, pause and try again - but only a couple times.
                    if (pauseCount <= 2)
                    {
                        pauseCount++;
                        await Task.Delay(100);
                        continue;
                    }
                    else
                    {
                        // No data, and we tried waiting for more, so we give up.
                        break;
                    }
                }

                // Since we received some data, we'll allow pausing for more data in future iterations.
                pauseCount = 0;

                // Add the latest bytes to the list.
                responseBytes.AddRange(buffer.Take(length));

                // Look for the end-of-response bytes.
                if (responseBytes.Count > 2)
                {
                    if ((responseBytes[length - 3] == '\r') &&
                        (responseBytes[length - 2] == '\r') &&
                        (responseBytes[length - 1] == '>'))
                    {
                        // Hopefully the loop will always exit here.
                        responseBytes.RemoveRange(responseBytes.Count - 3, 3);
                        break;
                    }
                }
            }
            
            string response = Encoding.ASCII.GetString(responseBytes.ToArray(), 0, responseBytes.Count);
            this.Logger.AddDebugMessage("Received " + response);
            return response;
        }
    }
}
