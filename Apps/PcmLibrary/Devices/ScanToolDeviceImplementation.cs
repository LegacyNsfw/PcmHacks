using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// This class encapsulates all code that is unique to the ScanTool MX interface.
    /// </summary>
    public class ScanToolDeviceImplementation : ElmDeviceImplementation
    {
        /// <summary>
        /// Device type for use in the Device Picker dialog box, and for internal comparisons.
        /// </summary>
        public const string DeviceType = "ObdLink ScanTool";
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ScanToolDeviceImplementation(
            Action<Message> enqueue,
            Func<int> getRecievedMessageCount,
            IPort port, 
            ILogger logger) : 
            base(enqueue, getRecievedMessageCount, port, logger)
        {
            // Both of these numbers could be slightly larger, but round numbers are easier to work with,
            // and these are only used with the Scantool SX interface anyhow. If we detect an AllPro
            // adapter we'll overwrite these values, see the Initialize method below.

            // Please keep the left side easy to read in hex. Then add 12 bytes for VPW overhead.
            // The STPX approach to sending messages should work with larger buffers, but when I tried
            // with my SX, it didn't work. That might only work with the MX (bluetooth version).
            this.MaxSendSize = 192 + 12;

            // The ScanTool SX will download 512kb in roughly 30 minutes at 500 bytes per read.
            // ScanTool reliability suffers at 508 bytes or more, so we're going with a number
            // that's round in base 10 rather than in base 2.
            this.MaxReceiveSize = 500 + 12;

            // This would need a firmware upgrade at the very least, and likely isn't even possible 
            // with current hardware.
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
        /// Confirm that we're actually connected to the right device, and initialize it.
        /// </summary>
        public override async Task<bool> Initialize()
        {
            this.Logger.AddDebugMessage("Determining whether " + this.ToString() + " is connected.");
            
            try
            {
                string stID = await this.SendRequest("ST I");                 // Identify (ScanTool.net)
                if (stID == "?" || string.IsNullOrEmpty(stID))
                {
                    this.Logger.AddDebugMessage("This is not a ScanTool device.");
                    return false;
                }

                this.Logger.AddUserMessage("ScanTool device ID: " + stID);

                if (stID.Contains("STN1130")) // SX
                {
                    this.MaxSendSize = 192 + 12;
                    this.MaxReceiveSize = 500 + 12;
                }
                else if (stID.Contains("STN1110") || // SparkFun OBD-II UART
                    stID.Contains("STN1150") || // MX version 1
                    stID.Contains("STN1151") || // MX version 2
                    stID.Contains("STN1155") || // LX
                    stID.Contains("STN2255"))   // MX+
                {
                    // 2048 works, but doesn't write measurably faster.
                    this.MaxSendSize = 1024 + 12;
                    this.MaxReceiveSize = 1024 + 12;
                }
                else
                {
                    this.Logger.AddUserMessage("This ScanTool device is not supported.");
                    this.Logger.AddUserMessage("Please check pcmhammer.org to ensure that you have the latest release.");
                    this.Logger.AddUserMessage("We're going to default to very small packet sizes, which will make everything slow, but at least it'll probably work.");
                    this.MaxSendSize = 128 + 12;
                    this.MaxReceiveSize = 128 + 12;
                }

                // Setting timeout to maximum. Since we use STPX commands, the device will stop
                // listening when it receives the expected number of responses, rather than 
                // waiting for the timeout.
                this.Logger.AddDebugMessage(await this.SendRequest("AT ST FF"));

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
                        milliseconds = 50;
                        break;

                    case TimeoutScenario.EraseMemoryBlock:
                        milliseconds = 1000;
                        break;

                    case TimeoutScenario.WriteMemoryBlock:
                        milliseconds = 140; // 125 works, added some for safety
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

                    case TimeoutScenario.Maximum:
                        return 1020;

                    default:
                        throw new NotImplementedException("Unknown timeout scenario " + scenario);
                }
            }
            else
            {
                throw new NotImplementedException("Since when did ScanTool devices support 4x?");
            }

            return milliseconds;
        }

        /// <summary>
        /// Send a message, do not expect a response.
        /// </summary>
        /// <remarks>
        /// This initially used standard ELM commands, however the ScanTool family
        /// of devices supports an "STPX" command that simplifies things a lot.
        /// Timeout adjustements are no longer needed, and longer packets are supported.
        /// </remarks>
        public override async Task<bool> SendMessage(Message message)
        {
            byte[] messageBytes = message.GetBytes();

            StringBuilder builder = new StringBuilder();
            builder.Append("STPX H:");
            builder.Append(messageBytes[0].ToString("X2"));
            builder.Append(messageBytes[1].ToString("X2"));
            builder.Append(messageBytes[2].ToString("X2"));

            int responses;
            switch (this.TimeoutScenario)
            {
                case TimeoutScenario.DataLogging3:
                    responses = 3;
                    break;

                case TimeoutScenario.DataLogging2:
                    responses = 2;
                    break;

                default:
                    responses = 1;
                    break;
            }

            // Special case for tool-present broadcast messages.
            // TODO: Create a new TimeoutScenario value, maybe call it "TransmitOnly" or something like that.
            if (Utility.CompareArrays(messageBytes, 0x8C, 0xFE, 0xF0, 0x3F))
            {
                responses = 0;
            }

            builder.AppendFormat(", R:{0}", responses);

            if (messageBytes.Length < 200)
            {
                // Short messages can be sent with a single write to the ScanTool.
                builder.Append(", D:");
                for(int index = 3; index < messageBytes.Length; index++)
                {
                    builder.Append(messageBytes[index].ToString("X2"));
                }

                string dataResponse = await this.SendRequest(builder.ToString());
                if (!this.ProcessResponse(dataResponse, "STPX with data", allowEmpty: responses == 0))
                {
                    if (dataResponse == string.Empty || dataResponse == "STOPPED" || dataResponse == "?")
                    {
                        // These will happen if the bus is quiet, for example right after uploading the kernel.
                        // They are traced during the SendRequest code. No need to repeat that message.
                    }
                    else
                    {
                        this.Logger.AddUserMessage("Unexpected response to STPX with data: " + dataResponse);
                    }
                    return false;
                }
            }
            else
            {
                // Long messages need to be sent in two steps: first the STPX command, then the data payload.
                builder.Append(", L:");
                int dataLength = messageBytes.Length - 3;
                builder.Append(dataLength.ToString());

                string header = builder.ToString();
                for (int attempt = 1; attempt <= 5; attempt++)
                {
                    string headerResponse = await this.SendRequest(header);
                    if (headerResponse != "DATA")
                    {
                        this.Logger.AddUserMessage("Unexpected response to STPX header: " + headerResponse);
                        continue;
                    }

                    break;
                }

                builder = new StringBuilder();
                for (int index = 3; index < messageBytes.Length; index++)
                {
                    builder.Append(messageBytes[index].ToString("X2"));
                }

                string data = builder.ToString();
                string dataResponse = await this.SendRequest(data);

                if (!this.ProcessResponse(dataResponse, "STPX payload", responses == 0))
                {
                    this.Logger.AddUserMessage("Unexpected response to STPX payload: " + dataResponse);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Borrowed from the AllPro class just for testing. Should be removed after STPX is working.
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
        public override async Task Receive()
        {
            try
            {
                string response = await this.ReadELMLine();
                this.ProcessResponse(response, "receive");

                if (this.getRecievedMessageCount() == 0)
                {
                   // await this.ReceiveViaMonitorMode();
                }
            }
            catch (TimeoutException)
            {
                this.Logger.AddDebugMessage("Timeout during receive.");
                // await this.ReceiveViaMonitorMode();
            }
        }

        /// <summary>
        /// This doesn't actually work yet, but I like the idea...
        /// </summary>
        private async Task ReceiveViaMonitorMode()
        {
            try
            {
                string monitorResponse = await this.SendRequest("AT MA");
                this.Logger.AddDebugMessage("Response to AT MA 1: " + monitorResponse);

                if (monitorResponse != ">?")
                {
                    string response = await this.ReadELMLine();
                    this.ProcessResponse(monitorResponse, "receive via monitor");
                }
            }
            catch(TimeoutException)
            {
                this.Logger.AddDebugMessage("Timeout during receive via monitor mode.");
            }
            finally
            { 
                string stopMonitorResponse = await this.SendRequest("AT MA");
                this.Logger.AddDebugMessage("Response to AT MA 2: " + stopMonitorResponse);
            }
        }
    }
}