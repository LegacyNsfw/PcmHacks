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
            configuration.Timeout = 1000;

            await this.Port.OpenAsync(configuration);
            await this.Port.DiscardBuffers();

            try
            {
                // Reset
                this.Logger.AddDebugMessage(await this.SendRequest("AT Z"));  // reset
                this.Logger.AddDebugMessage(await this.SendRequest("AT E0")); // disable echo
                this.Logger.AddDebugMessage(await this.SendRequest("AT S0")); // no spaces on responses

                string elmId = await this.SendRequest("AT I");                // Identify (ELM)
                this.Logger.AddUserMessage("Device supports " + elmId);

                string stId = await this.SendRequest("ST I");                 // Identify (ScanTool.net)
                this.Logger.AddUserMessage("Device supports " + stId);

                string allproId = await this.SendRequest("AT #1");            // Identify (AllPro)
                this.Logger.AddUserMessage("All Pro: " + allproId);

                string voltage = await this.SendRequest("AT RV");             // Get Voltage
                this.Logger.AddUserMessage("Voltage: " + voltage);

                if (!await this.SendAndVerify("AT AL", "OK") ||               // Allow Long packets
                    !await this.SendAndVerify("AT SP2", "OK") ||              // Set Protocol 2 (VPW)
                    !await this.SendAndVerify("AT DP", "SAE J1850 VPW"))      // Get Protocol (Verify VPW)
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

                this.Logger.AddDebugMessage("hexResponse: " + hexResponse);
                // Make sure we can parse the response.
                if (!hexResponse.IsHex())
                {
                    this.Logger.AddDebugMessage("Unexpected response: " + hexResponse);
                    return Response.Create(ResponseStatus.UnexpectedResponse, (Message)null);
                }

                // Add the header bytes that the ELM hides from us.
                byte[] deviceResponseBytes = hexResponse.ToBytes();
                this.Logger.AddDebugMessage("deviceResponseBytes: " + deviceResponseBytes.ToHex());
                byte[] completeResponseBytes = new byte[deviceResponseBytes.Length + 3];
                completeResponseBytes[0] = messageBytes[0];
                completeResponseBytes[1] = messageBytes[2];
                completeResponseBytes[2] = messageBytes[1];
                deviceResponseBytes.CopyTo(completeResponseBytes, 3);

                this.Logger.AddDebugMessage("Recieved: " + completeResponseBytes.ToHex());

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
                this.Logger.AddDebugMessage(actualResponse + "=" + expectedResponse);
                return true;
            }

            this.Logger.AddDebugMessage("Bad. " + actualResponse + " does not equal " + expectedResponse);
            return false;
        }

        /// <summary>
        /// Reads and filteres a line from the device, returns it as a string
        /// </summary>
        async private Task<Response<string>> ReadELMLine()
        {
            // 4112 is max packet size on the AVT, use it here too. *3 because of the ASCII encoding and spaces, add 8 for additional header bytes?
            // plus 2 so we can read a CR or LF and if we're still reading by the last byte it's an error
            int max = 4112 * 3 + 8 + 1;
            byte[] buffer = new byte[max];

            // collect bytes until we hit the end of the buffer or see a CR or LF
            int i = 0;
            byte[] b = new byte[1]; // FIXME: If I dont copy to this buffer, and instead use buffer[i] inline in the next loop, the test for '>' does not work in the while clause.
            do
            {
                await this.Port.Receive(b, 0, 1);
                //this.Logger.AddDebugMessage("Byte: " + b[0].ToString("X2") + " Ascii: " + System.Text.Encoding.ASCII.GetString(b));
                buffer[i] = b[0];
                i++;
            } while ((i < max) && (b[0] != '>')); // continue until the next prompt

            //this.Logger.AddDebugMessage("Found terminator '>'");

            // count the wanted bytes
            int wanted = 0;
            int j;
            for (j=0; j<i; j++)
            {
                if (buffer[j] >= 32 && buffer[j] <= 126 && buffer[j] != '>') wanted++; // printable characters only, and not '>'
            }

            //this.Logger.AddDebugMessage(wanted + " bytes to keep from " + i);

            // build a message of the correct length
            // i is the length of the data in the original buffer
            // j is pointer to the offset in the filtered buffer
            // k is the pointer in to the original buffer
            int k;
            byte[] filtered = new byte[wanted]; // create a new filtered buffer of the correct size
            for (k=0, j=0; k<i; k++) // start both buffers from 0, always increment the original buffer 
            {
                if (buffer[k] >= 32 && buffer[k] <= 126 && buffer[k] != '>') // do we want THIS byte?
                {
                    b[0] = buffer[k];
                    //this.Logger.AddDebugMessage("Filtered Byte: " + buffer[k].ToString("X2") + " Ascii: " + System.Text.Encoding.ASCII.GetString(b));
                    filtered[j++] = buffer[k];  // save it, and increment the pointer in the filtered buffer
                }
            }

            //this.Logger.AddDebugMessage("built filtered string kept " + j + " bytes filtered is " + filtered.Length + " long");

            //this.Logger.AddDebugMessage("filtered: " + filtered.ToHex());
            string line = System.Text.Encoding.ASCII.GetString(filtered);

            this.Logger.AddDebugMessage("Read \"" + line + "\"");                          

            if (i == max) return Response.Create(ResponseStatus.Truncated, line);

            return Response.Create(ResponseStatus.Success, line);
        }

        /// <summary>
        /// Collects a line with ReadELMLine() and converts it to a Message
        /// </summary>
        async private Task<Response<Message>> ReadELMPacket()
        {
            this.Logger.AddDebugMessage("Trace: ReadELMPacket");

            Response<string> response = await ReadELMLine();
            byte[] message = response.Value.ToBytes();

            return Response.Create(ResponseStatus.Success, new Message(message));
        }

        /// <summary>
        /// Send a request in string form, wait for a response (for init)
        /// </summary>
        /// <remarks>
        /// The API for this method (sending a string, returning a string) matches
        /// the way that we need to communicate with ELM and STN devices for setup
        /// </remarks>
        private async Task<string> SendRequest(string request)
        {
            this.Logger.AddDebugMessage("Sending " + request);
            await this.Port.Send(Encoding.ASCII.GetBytes(request + "\r\n"));

            Response<string> response = await ReadELMLine();

            return response.Value;
        }
    }
}
