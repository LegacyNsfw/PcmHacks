using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// This class encapsulates code that is shared by AllProDevice and ScanToolDevice.
    /// </summary>
    public class ElmDeviceImplementation
    {
        protected IPort Port { get; private set; }

        protected ILogger Logger { get; private set; }

        public int MaxSendSize { get; protected set; }

        public int MaxReceiveSize { get; protected set; }

        public bool Supports4X { get; protected set; }

        protected readonly Action<Message> enqueue;

        protected readonly Func<int> getRecievedMessageCount;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElmDeviceImplementation(
            Action<Message> enqueue,
            Func<int> getRecievedMessageCount,
            IPort port, 
            ILogger logger)
        {
            this.enqueue = enqueue;
            this.getRecievedMessageCount = getRecievedMessageCount;
            this.Port = port;
            this.Logger = logger;

            // These are only relevant for device initialization.
            // After that, configuration from the derived classes will be used instead.
            this.MaxReceiveSize = 200;
            this.MaxSendSize = 200;
            this.Supports4X = false;
        }

        /// <summary>
        /// This string is what will appear in the drop-down list in the UI.
        /// </summary>
        public virtual string GetDeviceType()
        {
            throw new NotImplementedException("This is only implemented by derived classes.");
        }

        /// <summary>
        /// Confirm that we're actually connected to the right device, and initialize it.
        /// </summary>
        public virtual async Task<bool> Initialize()
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

            return true;
        }

        /// <summary>
        /// Send a message, do not expect a response.
        /// </summary>
        public virtual Task<bool> SendMessage(Message message)
        {
            throw new NotImplementedException("This is only implemented by derived classes.");
        }

        /// <summary>
        /// Try to read an incoming message from the device.
        /// </summary>
        public virtual Task Receive()
        {
            throw new NotImplementedException("This is only implemented by derived classes.");
        }

        /// <summary>
        /// Send a request in string form, wait for a response (for init)
        /// </summary>
        /// <remarks>
        /// The API for this method (sending a string, returning a string) matches
        /// the way that we need to communicate with ELM and STN devices for setup
        /// </remarks>
        public async Task<string> SendRequest(string request)
        {
            this.Logger.AddDebugMessage("TX: " + request);
            await this.Port.Send(Encoding.ASCII.GetBytes(request + " \r"));

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
        public async Task<bool> SendAndVerify(string message, string expectedResponse)
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
        /// Process responses from the EML/ST devices.
        /// </summary>
        protected bool ProcessResponse(string rawResponse, string context, bool allowEmpty = false)
        {
            if (string.IsNullOrWhiteSpace(rawResponse))
            {
                this.Logger.AddDebugMessage(
                    string.Format("Empty response to {0}. {1}",
                    context,
                    allowEmpty ? "That's OK" : "That's not OK."));

                if (allowEmpty)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (rawResponse == "OK")
            {
                return true;
            }

            // We sent successfully, but the PCM didn't reply immediately.
            if (rawResponse == "NO DATA")
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
                        this.enqueue(response);
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

    }
}