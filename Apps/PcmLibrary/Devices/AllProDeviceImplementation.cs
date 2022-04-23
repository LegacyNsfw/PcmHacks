using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// This class encapsulates all code that is unique to the AllPro USB interface.
    /// </summary>
    public class AllProDeviceImplementation : ElmDeviceImplementation
    {
        /// <summary>
        /// Device type for use in the Device Picker dialog box, and for internal comparisons.
        /// </summary>
        public const string DeviceType = "AllPro";
        
        /// <summary>
        /// The device can cache the message header to speed up serial communications. 
        /// To use that properly, we need to keep track of the cached header.
        /// </summary>
        private string currentHeader = "unset";

        /// <summary>
        /// Constructor.
        /// </summary>
        public AllProDeviceImplementation(
            Action<Message> enqueue,
            Func<int> getRecievedMessageCount,
            IPort port,
            ILogger logger) :
            base(enqueue, getRecievedMessageCount, port, logger)
        {
            // Please keep the left side easy to read in hex. Then add 12 bytes for VPW overhead.
            this.MaxSendSize = 1024 + 12;
            this.MaxReceiveSize = 1024 + 12;
            this.Supports4X = true;

            // For this to work, the AllPro would need a firmware update that
            // adds ATMA support so that we could monitor the bus and log data. 
            this.SupportsStreamLogging = false;
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
            this.currentHeader = "header not yet set";

            try
            {
                string apID = await this.SendRequest("AT #1");                // Identify (AllPro)
                if (apID == "?")
                {
                    this.Logger.AddDebugMessage("This is not an AllPro device.");
                    return false;
                }

                this.Logger.AddUserMessage("All Pro ID: " + apID);
                this.Logger.AddUserMessage("All Pro self test result: " + await this.SendRequest("AT #3"));  // self test
                this.Logger.AddUserMessage("All Pro firmware: " + await this.SendRequest("AT @1"));          // firmware check
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
        /// Get the time required for the given scenario.
        /// </summary>
        public override int GetTimeoutMilliseconds(TimeoutScenario scenario, VpwSpeed speed)
        {
            int milliseconds;

            if (speed == VpwSpeed.Standard)
            {
                switch (scenario)
                {
                    case TimeoutScenario.Minimum:
                        milliseconds = 0;
                        break;

                    case TimeoutScenario.ReadProperty:
                        milliseconds = 25;
                        break;

                    case TimeoutScenario.ReadCrc:
                        milliseconds = 100;
                        break;

                    case TimeoutScenario.ReadMemoryBlock:
                        milliseconds = 300;
                        break;

                    case TimeoutScenario.EraseMemoryBlock:
                        milliseconds = 7000;
                        break;

                    case TimeoutScenario.WriteMemoryBlock:
                        milliseconds = 250;
                        break;

                    case TimeoutScenario.SendKernel:
                        milliseconds = 50;
                        break;

                    case TimeoutScenario.DataLogging1:
                        milliseconds = 25;
                        break;

                    case TimeoutScenario.DataLogging2:
                        milliseconds = 40;
                        break;

                    case TimeoutScenario.DataLogging3:
                        milliseconds = 60;
                        break;

                    case TimeoutScenario.DataLogging4:
                        milliseconds = 75;
                        break;

                    case TimeoutScenario.DataLoggingStreaming:
                        milliseconds = 0;
                        break;

                    case TimeoutScenario.Maximum:
                        return 1020;

                    default:
                        throw new NotImplementedException("Unknown timeout scenario " + scenario);
                }
            }
            else
            {
                switch (scenario)
                {
                    case TimeoutScenario.Minimum:
                        milliseconds = 0;
                        break;

                    // The app doesn't currently do this in 4X mode, so this is only a guess.
                    case TimeoutScenario.ReadProperty:
                        milliseconds = 12;
                        break;

                    case TimeoutScenario.ReadCrc:
                        milliseconds = 100;
                        break;

                    case TimeoutScenario.ReadMemoryBlock:
                        milliseconds = 50;
                        break;

                    case TimeoutScenario.EraseMemoryBlock:
                        milliseconds = 7000;
                        break;

                    case TimeoutScenario.WriteMemoryBlock:
                        milliseconds = 170;
                        break;

                    case TimeoutScenario.SendKernel:
                        milliseconds = 10;
                        break;

                    case TimeoutScenario.DataLogging1:
                        milliseconds = 7;
                        break;

                    case TimeoutScenario.DataLogging2:
                        milliseconds = 10;
                        break;

                    case TimeoutScenario.DataLogging3:
                        milliseconds = 15;
                        break;

                    case TimeoutScenario.DataLogging4:
                        milliseconds = 25;
                        break;

                    case TimeoutScenario.DataLoggingStreaming:
                        milliseconds = 15;
                        break;

                    case TimeoutScenario.Maximum:
                        return 1020;

                    default:
                        throw new NotImplementedException("Unknown timeout scenario " + scenario);
                }
            }

            return milliseconds;
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

            if (header != this.currentHeader)
            {
                string setHeaderResponse = await this.SendRequest("AT SH " + header);
                this.Logger.AddDebugMessage("Set header response: " + setHeaderResponse);

                if(setHeaderResponse == "STOPPED")
                {
                    // Does it help to retry once?
                    setHeaderResponse = await this.SendRequest("AT SH " + header);
                    this.Logger.AddDebugMessage("Set header response: " + setHeaderResponse);
                }

                if (!this.ProcessResponse(setHeaderResponse, "set-header command"))
                {
                    return false;
                }

                this.currentHeader = header;
            }

            payload = payload.Replace(" ", "");

            string sendMessageResponse = await this.SendRequest(payload + " ");
            if (!this.ProcessResponse(sendMessageResponse, "message content"))
            {
                return false;
            }

            return true;
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
        /// Try to read an incoming message from the device.
        /// </summary>
        /// <returns></returns>
        public override async Task Receive()
        {
            try
            {
                string response = await this.ReadELMLine();
                this.ProcessResponse(response, "receive");
            }
            catch (TimeoutException)
            {
                this.Logger.AddDebugMessage("Timeout during receive.");
            }
        }
    }
}