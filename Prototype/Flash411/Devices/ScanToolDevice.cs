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
    public class ScanToolDevice : SerialDevice
    {
        public const string DeviceType = "ObdLink or AllPro";
        string setheader = "unset";

        /// <summary>
        /// Constructor.
        /// </summary>
        public ScanToolDevice(IPort port, ILogger logger) : base(port, logger)
        {
            this.MaxSendSize = 140;    // Accuracy?
            this.MaxReceiveSize = 150; // Accuracy?
            this.Supports4X = false;
        }

        /// <summary>
        /// This string is what will appear in the drop-down list in the UI.
        /// </summary>
        public override string GetDeviceType()
        {
            return DeviceType;
        }

        /// <summary>
        /// Configure the device for use - and also confirm that the device is what we think it is.
        /// </summary>
        public override async Task<bool> Initialize()
        {
            this.Logger.AddDebugMessage("Initializing " + this.ToString());

            // We're going to reset the interface device, which means that it's going
            // to forgot what header the app previously told it to use. That requires
            // the app to forget what header the interface was told to use - that will
            // cause the app to send another set-header command later on.
            this.setheader = "header not yet set";
            
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

                // Device Identification
                string elmID = await this.SendRequest("AT I");                // Identify (ELM)
                string stID = await this.SendRequest("ST I");                 // Identify (ScanTool.net)
                string apID = await this.SendRequest("AT #1");                // Identify (AllPro)
                if (elmID != "?")
                {
                    this.Logger.AddUserMessage("Elm ID: " + elmID);
                    if (elmID.Contains("ELM327 v1.5"))
                    {
                        // TODO: Add a URL to a web page with a list of supported devices.
                        // No such web page exists yet, but I'm sure we'll create one some day...
                        this.Logger.AddUserMessage("ERROR: This OBD2 interface is not supported.");
                        return false;
                    }
                }

                if (stID != "?")
                {
                    this.Logger.AddUserMessage("ScanTool ID: " + stID);
                }

                if (apID != "?")
                {
                    this.Logger.AddUserMessage("All Pro ID: " + apID);
                    this.Logger.AddDebugMessage("All Pro self test result: " + await this.SendRequest("AT #3"));  // self test
                    this.Supports4X = true;
                }

                string voltage = await this.SendRequest("AT RV");             // Get Voltage
                this.Logger.AddUserMessage("Voltage: " + voltage);

                if (!await this.SendAndVerify("AT AL", "OK") ||               // Allow Long packets
                    !await this.SendAndVerify("AT SP2", "OK") ||              // Set Protocol 2 (VPW)
                    !await this.SendAndVerify("AT DP", "SAE J1850 VPW") ||    // Get Protocol (Verify VPW)
                    !await this.SendAndVerify("AT AR", "OK") ||               // Turn Auto Receive on (default should be on anyway)
                    !await this.SendAndVerify("AT ST 99", "OK") ||                  // Set timeout to N * 4 milliseconds - TODO: Adjust or remove!
                    !await this.SendAndVerify("AT SR " + DeviceId.Tool.ToString("X2"), "OK") || // Set receive filter to this tool ID
                    !await this.SendAndVerify("AT H1", "OK")                  // Send headers
                    )
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
        public override async Task<bool> SendMessage(Message message)
        {
            byte[] messageBytes = message.GetBytes();
            string header;
            string payload;
            this.ParseMessage(messageBytes, out header, out payload);

            Response<bool> setHeaderResponse = await this.SetHeader(header);
            if(setHeaderResponse.Status != ResponseStatus.Success)
            {
                return false;
            }

            string deviceResponse = await this.SendRequest(payload);
            this.Logger.AddDebugMessage("SendMessage produced " + deviceResponse);

            // TODO: Parse deviceResponse, determine whether we actually succeeded or failed.
            return true;
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
            byte[] messageBytes = message.GetBytes();
            string header;
            string payload;
            this.ParseMessage(messageBytes, out header, out payload);

            Response<bool> setHeaderResponse = await this.SetHeader(header);
            if (setHeaderResponse.Status != ResponseStatus.Success)
            {
                return Response.Create(setHeaderResponse.Status, (Message)null);
            }

            try
            {
                string hexResponse = await this.SendRequest(payload);

                //this.Logger.AddDebugMessage("hexResponse: " + hexResponse);
                // Make sure we can parse the response.
                if (!hexResponse.IsHex())
                {
                    this.Logger.AddDebugMessage("Unexpected response: " + hexResponse);
                    return Response.Create(ResponseStatus.UnexpectedResponse, (Message)null);
                }
                byte[] deviceResponseBytes = hexResponse.ToBytes();
                Array.Resize(ref deviceResponseBytes, deviceResponseBytes.Length - 1); // remove checksum byte
                this.Logger.AddDebugMessage("RX: " + deviceResponseBytes.ToHex());
                return Response.Create(ResponseStatus.Success, new Message(deviceResponseBytes));
            }
            catch (TimeoutException)
            {
                return Response.Create(ResponseStatus.Timeout, (Message)null);
            }
        }

        /// <summary>
        /// Read a message, without sending one first.
        /// </summary>
        public async override Task<Response<Message>> ReadMessage()
        {
            // TODO? Should we set a mesage filter here?
        //    if (!await this.SendAndVerify("ST M", "OK"))
            {
        //        return new Response<Message>(ResponseStatus.Error, null);
            }
            
            Response<string> stringResponse1;
            Response<string> stringResponse2;
            Response<string> stringResponse3;

            try
            {
                stringResponse1 = await this.ReadELMLine();
                stringResponse2 = await this.ReadELMLine();
                stringResponse3 = await this.ReadELMLine();
                this.ToString();
            }
            catch(Exception exception)
            {
                // Just a place to set a breakpoint.
                exception.ToString();
            }

            return new Response<Message>(ResponseStatus.Error, null);
        }

        /// <summary>
        /// Separate the message into header and payload.
        /// </summary>
        private void ParseMessage(byte[] messageBytes, out string header, out string payload)
        {
            // The incoming byte array needs to separated into header and payload portions,
            // which are sent separately.
            string hexRequest = messageBytes.ToHex();
            header = hexRequest.Substring(0, 9);
            payload = hexRequest.Substring(9);
        }

        /// <summary>
        /// Set the header that the Elm device will use for the next message.
        /// </summary>
        /// <remark>
        /// This is a no-op if the header has not changed since the last request, and just returns success
        /// </remark>
        public async Task<Response<bool>> SetHeader(string header)
        {
            if (header != this.setheader)
            {
                string setHeaderResponse = await this.SendRequest("AT SH " + header);
                if (setHeaderResponse != "OK")
                {
                    this.Logger.AddDebugMessage("Unexpected response to set-header command: " + setHeaderResponse);
                    return Response.Create(ResponseStatus.UnexpectedResponse, false);
                }
                this.setheader = header;
            }
            return Response.Create(ResponseStatus.Success, true);
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
            this.Logger.AddDebugMessage("TX: " + request);
            await this.Port.Send(Encoding.ASCII.GetBytes(request + "\r\n"));

            Response<string> response = await ReadELMLine();

            return response.Value;
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
                this.Logger.AddDebugMessage(actualResponse);
                return true;
            }

            this.Logger.AddDebugMessage("Did not recieve expected response. " + actualResponse + " does not equal " + expectedResponse);
            return false;
        }

        /// <summary>
        /// Reads and filteres a line from the device, returns it as a string
        /// </summary>
        /// <remarks>
        /// Strips Non Printable, >, and Line Feeds. Converts Carriage Return to Space. Strips leading and trailing whitespace.
        /// </remarks>
        async private Task<Response<string>> ReadELMLine()
        {
            int buffersize = (MaxReceiveSize * 3) + 7; // payload with spaces (3 bytes per character) plus the longest possible prompt
            byte[] buffer = new byte[buffersize];

            // collect bytes until we hit the end of the buffer or see a CR or LF
            int i = 0;
            byte[] b = new byte[1]; // FIXME: If I dont copy to this buffer, and instead use buffer[i] inline in the next loop, the test for '>' does not work in the while clause.
            do
            {
                await this.Port.Receive(b, 0, 1);
                //this.Logger.AddDebugMessage("Byte: " + b[0].ToString("X2") + " Ascii: " + System.Text.Encoding.ASCII.GetString(b));
                buffer[i] = b[0];
                i++;
            } while ((i < buffersize) && (b[0] != '>')); // continue until the next prompt

            //this.Logger.AddDebugMessage("Found terminator '>'");

            // count the wanted bytes and replace CR with space
            int wanted = 0;
            int j;
            for (j = 0; j < i; j++)
            {
                if (buffer[j] == 13) buffer[j] = 32; // CR -> Space
                if (buffer[j] >= 32 && buffer[j] <= 126 && buffer[j] != '>') wanted++; // printable characters only, and not '>'
            }

            //this.Logger.AddDebugMessage(wanted + " bytes to keep from " + i);

            // build a message of the correct length
            // i is the length of the data in the original buffer
            // j is pointer to the offset in the filtered buffer
            // k is the pointer in to the original buffer
            int k;
            byte[] filtered = new byte[wanted]; // create a new filtered buffer of the correct size
            for (k = 0, j = 0; k < i; k++) // start both buffers from 0, always increment the original buffer 
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
            string line = System.Text.Encoding.ASCII.GetString(filtered).Trim(); // strip leading and trailing whitespace, too

            //this.Logger.AddDebugMessage("Read \"" + line + "\"");

            if (i == buffersize) return Response.Create(ResponseStatus.Truncated, line);

            return Response.Create(ResponseStatus.Success, line);
        }

        /// <summary>
        /// Collects a line with ReadELMLine() and converts it to a Message
        /// </summary>
        async private Task<Response<Message>> ReadELMPacket()
        {
            //this.Logger.AddDebugMessage("Trace: ReadELMPacket");

            Response<string> response = await ReadELMLine();
            byte[] message = response.Value.ToBytes();

            return Response.Create(ResponseStatus.Success, new Message(message));
        }

        /// <summary>
        /// Set the interface to low (false) or high (true) speed
        /// </summary>
        /// <remarks>
        /// The caller must also tell the PCM to switch speeds
        /// </remarks>
        public override async Task<bool> SetVPW4x(bool highspeed)
        {
            if (highspeed != true)
            {
                this.Logger.AddDebugMessage("AllPro setting VPW 1X");
                if (!await this.SendAndVerify("AT VPW1", "OK"))
                    return false;
            }
            else
            {
                this.Logger.AddDebugMessage("AllPro setting VPW 4X");
                if (!await this.SendAndVerify("AT VPW4", "OK"))
                    return false;
            }

            return true;
        }
    }
}