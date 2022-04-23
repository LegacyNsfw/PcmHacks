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

            // In theory we could use ATMA or STMA to monitor the bus and read data log streams.
            // In practice I couldn't get that to work. See SetTimeout & SetTimeoutMilliseconds.
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

                // The following table was provided by ScanTool.net Support - ticket #33419
                // Device                     Max Msg Size    Max Tested Baudrate
                // STN1110                    2k              2 Mbps *
                // STN1130 (OBDLink SX)       2k              2 Mbps *
                // STN1150 (OBDLink MX v1)    2k              N/A
                // STN1151 (OBDLink MX v2)    2k              N/A
                // STN1155 (OBDLink LX)       2k              N/A
                // STN1170                    2k              2 Mbps *
                // STN2100                    4k              2 Mbps
                // STN2120                    4k              2 Mbps
                // STN2230 (OBDLink EX)       4k              N/A
                // STN2255 (OBDLink MX+)      4k              N/A
                //
                // * With character echo off (ATE 0), 1 Mbps with character echo on (ATE 1)

                // Here 1024 bytes = 2048 ASCII hex bytes, without spaces
                if (stID.Contains("STN1110") || // SparkFun OBD-II UART
                    stID.Contains("STN1130") || // SX
                    stID.Contains("STN1150") || // MX v1
                    stID.Contains("STN1151") || // MX v2
                    stID.Contains("STN1155") || // LX
                    stID.Contains("STN1170") || //
                    stID.Contains("STN2100") || //
                    stID.Contains("STN2120") || //
                    stID.Contains("STN2230") || // EX
                    stID.Contains("STN2255"))   // MX+
                {
                    // Testing shows larger packet sizes do not equate to faster transfers
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

                // Setting timeout to a large value. Since we use STPX commands,
                // the device will stop listening when it receives the expected
                // number of responses, rather than waiting for the timeout.
                this.Logger.AddDebugMessage(await this.SendRequest("STPTO 3000"));

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
                        milliseconds = 250;
                        break;

                    case TimeoutScenario.EraseMemoryBlock:
                        milliseconds = 7000;
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

                    case TimeoutScenario.DataLogging4:
                        milliseconds = 80;
                        break;

                    case TimeoutScenario.DataLoggingStreaming:
                        // This is hacky, but the code path is not supported anyway.
                        // I had hoped to use ATMA or STMA to monitor the bus and log
                        // data, but that hasn't worked.  Also see SetTimeoutMilliseconds.
                        milliseconds = -1;
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
        /// Set the timeout to the device. If this is set too low, the device
        /// will return 'No Data'. The ST Equivalent timeout command doesn't have
        /// the same 1020 millisecond limit since it takes an integer milliseconds
        /// as a paramter.
        /// </summary>
        public override async Task<bool> SetTimeoutMilliseconds(int milliseconds)
        {
            if (milliseconds == -1)
            {
                // This doesn't actually work yet, but I think it should be possible.
                // To test this code path, change this value in the constructor:
                // this.SupportsStreamLogging = false;
                return await this.SendAndVerify("STMA", "");
            }
            else
            {
                return await this.SendAndVerify("STPTO " + milliseconds, "OK");
            }           
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
                case TimeoutScenario.DataLogging4:
                    responses = 4;
                    break;

                case TimeoutScenario.DataLogging3:
                    responses = 3;
                    break;

                case TimeoutScenario.DataLogging2:
                    responses = 2;
                    break;

                case TimeoutScenario.DataLogging1:
                    responses = 1;
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

            if (this.TimeoutScenario != TimeoutScenario.DataLoggingStreaming)
            {
                builder.AppendFormat(", R:{0}", responses);
            }

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