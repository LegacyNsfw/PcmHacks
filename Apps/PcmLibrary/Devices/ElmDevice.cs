using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// This class encapsulates common code for ELM-derived devices, and also handles detecting the
    /// specific ELM device that is attached. After detecting the device is acts as a facade, with
    /// a device-specific class implementing device-specific functionality.
    /// </summary>
    public class ElmDevice : SerialDevice
    {
        /// <summary>
        /// Device type for use in the Device Picker dialog box, and for internal comparisons.
        /// </summary>
        public const string DeviceType = "ObdLink or AllPro";

        /// <summary>
        /// Timeout periods vary depending on the current usage scenario.
        /// This indicates which scenariow was configured most recently.
        /// </summary>
        private TimeoutScenario currentTimeout = TimeoutScenario.Undefined;

        /// <summary>
        /// This will be initalized after discovering which device is actually connected at the moment.
        /// </summary>
        private SerialDevice implementation = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElmDevice(IPort port, ILogger logger) : base(port, logger)
        {
        }

        /// <summary>
        /// This string is what will appear in the drop-down list in the UI.
        /// </summary>
        public override string GetDeviceType()
        {
            if (this.implementation == null)
            {
                return DeviceType;
            }

            return this.implementation.GetDeviceType();
        }

        /// <summary>
        /// Use the related classes to discover which type of device is currently connected.
        /// </summary>
        public override async Task<bool> Initialize()
        {
            this.Logger.AddDebugMessage("ElmDevice initialization starting.");
            
            SerialPortConfiguration configuration = new SerialPortConfiguration();
            configuration.BaudRate = 115200;
            configuration.Timeout = 1200;

            await this.Port.OpenAsync(configuration);
            await this.Port.DiscardBuffers();

            try
            {
                // This is common across all ELM-based devices.
                await this.SendRequest(""); // send a cr/lf to prevent the ATZ failing.
                this.Logger.AddDebugMessage(await this.SendRequest("AT Z"));  // reset
                this.Logger.AddDebugMessage(await this.SendRequest("AT E0")); // disable echo
                this.Logger.AddDebugMessage(await this.SendRequest("AT S0")); // no spaces on responses

                string voltage = await this.SendRequest("AT RV");             // Get Voltage
                this.Logger.AddUserMessage("Voltage: " + voltage);

                // First we check for known-bad ELM clones.
                string elmID = await this.SendRequest("AT I");                // Identify (ELM)
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

                AllProDevice allProDevice = new AllProDevice(this.Port, this.Logger);
                if (await allProDevice.Initialize())
                {
                    this.implementation = allProDevice;
                }

                ScanToolDevice scanToolDevice = new ScanToolDevice(this.Port, this.Logger);
                if (await scanToolDevice.Initialize())
                {
                    this.implementation = scanToolDevice;
                }

                // These are shared by all ELM-based devices.
                if (!await this.SendAndVerify("AT AL", "OK") ||               // Allow Long packets
                    !await this.SendAndVerify("AT SP2", "OK") ||              // Set Protocol 2 (VPW)
                    !await this.SendAndVerify("AT DP", "SAE J1850 VPW") ||    // Get Protocol (Verify VPW)
                    !await this.SendAndVerify("AT AR", "OK") ||               // Turn Auto Receive on (default should be on anyway)
                    !await this.SendAndVerify("AT AT0", "OK") ||              // Disable adaptive timeouts
                    !await this.SendAndVerify("AT SR " + DeviceId.Tool.ToString("X2"), "OK") || // Set receive filter to this tool ID
                    !await this.SendAndVerify("AT H1", "OK") ||               // Send headers
                    !await this.SendAndVerify("AT ST 20", "OK")               // Set timeout (will be adjusted later, too)                 
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
        /// Set the amount of time that we'll wait for a message to arrive.
        /// </summary>
        public override async Task SetTimeout(TimeoutScenario scenario)
        {
            if (this.currentTimeout == scenario)
            {
                return;
            }

            int milliseconds = this.GetVpwTimeoutMilliseconds(scenario);
            
            this.Logger.AddDebugMessage("Setting timeout for " + scenario + ", " + milliseconds.ToString() + " ms.");

            // The port timeout needs to be considerably longer than the device timeout,
            // otherwise you get "STOPPED" or "NO DATA" somewhat randomly. (I mostly saw
            // this when sending the tool-present messages, but that might be coincidence.)
            //
            // 100 was not enough
            // 150 seems like enough
            // Consider 200 if STOPPED / NO DATA is still a problem. 
            this.Port.SetTimeout(milliseconds + 150);

            // I briefly tried hard-coding timeout values for the AT ST command,
            // but that's a recipe for failure. If the port timeout is shorter
            // than the device timeout, reads will consistently fail.
            int parameter = Math.Min(Math.Max(1, (milliseconds / 4)), 255);
            string value = parameter.ToString("X2");
            await this.SendAndVerify("AT ST " + value, "OK");
        }

        /// <summary>
        /// Send a message, do not expect a response.
        /// </summary>
        public override async Task<bool> SendMessage(Message message)
        {
            return await this.implementation.SendMessage(message);
        }

        /// <summary>
        /// Try to read an incoming message from the device.
        /// </summary>
        /// <returns></returns>
        protected override async Task Receive()
        {
            await this.Receive();
        }
        
        /// <summary>
        /// Process responses from the EML/ST devices.
        /// </summary>
        protected bool ProcessResponse(string rawResponse, string context)
        {
            if (string.IsNullOrWhiteSpace(rawResponse))
            {
                this.Logger.AddDebugMessage(
                    string.Format("Empty response to {0}.",
                    context));
                return false;
            }

            if (rawResponse == "OK")
            {
                return true;
            }

            string[] segments = rawResponse.Split('<');
            foreach (string segment in segments)
            {
                if (segment.IsHex())
                {
                    string[] hexResponses = segment.Split(' ');
                    foreach (string singleHexResponse in hexResponses)
                    {
                        byte[] deviceResponseBytes = singleHexResponse.ToBytes();
                        if (deviceResponseBytes.Length > 0)
                        {
                            Array.Resize(ref deviceResponseBytes, deviceResponseBytes.Length - 1); // remove checksum byte
                        }

                        this.Logger.AddDebugMessage("RX: " + deviceResponseBytes.ToHex());

                        Message response = new Message(deviceResponseBytes);
                        this.Enqueue(response);
                    }

                    return true;
                }

                if (segment.EndsWith("OK"))
                {
                    this.Logger.AddDebugMessage("WTF: Response not valid, but ends with OK.");
                    return true;
                }

                this.Logger.AddDebugMessage(
                    string.Format(
                        "Unexpected response to {0}: {1}",
                        context,
                        segment));
            }

            return false;
        }
        
        /// <summary>
        /// Send a request in string form, wait for a response (for init)
        /// </summary>
        /// <remarks>
        /// The API for this method (sending a string, returning a string) matches
        /// the way that we need to communicate with ELM and STN devices for setup
        /// </remarks>
        protected async Task<string> SendRequest(string request)
        {
            this.Logger.AddDebugMessage("TX: " + request);
            await this.Port.Send(Encoding.ASCII.GetBytes(request + "\r\n"));

            try
            {
                string response = await ReadELMLine();

                return response;
            }
            catch (TimeoutException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Send a command to the device, confirm that we got the response we expect. 
        /// </summary>
        /// <remarks>
        /// This is primarily for use in the Initialize method, where we're talking to the 
        /// interface device rather than the PCM.
        /// </remarks>
        protected async Task<bool> SendAndVerify(string message, string expectedResponse)
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
        /// Reads and filters a line from the device, returns it as a string
        /// </summary>
        /// <remarks>
        /// Strips Non Printable, >, and Line Feeds. Converts Carriage Return to Space. Strips leading and trailing whitespace.
        /// </remarks>
        protected async Task<string> ReadELMLine()
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

            return line;
        }

        /// <summary>
        /// Collects a line with ReadELMLine() and converts it to a Message
        /// </summary>
        protected async Task<Response<Message>> ReadELMPacket()
        {
            //this.Logger.AddDebugMessage("Trace: ReadELMPacket");

            string response = await ReadELMLine();

            byte[] message = response.ToBytes();

            return Response.Create(ResponseStatus.Success, new Message(message));
        }

        /// <summary>
        /// Set the interface to low (false) or high (true) speed
        /// </summary>
        /// <remarks>
        /// The caller must also tell the PCM to switch speeds
        /// </remarks>
        protected override async Task<bool> SetVpwSpeedInternal(VpwSpeed newSpeed)
        {
            if (newSpeed == VpwSpeed.Standard)
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

        /// <summary>
        /// Discard any messages in the recevied-message queue.
        /// </summary>
        public override void ClearMessageBuffer()
        {
            this.Port.DiscardBuffers();
        }
    }
}