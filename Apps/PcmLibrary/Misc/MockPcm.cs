using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// Simulate a PCM for testing without a car or bench PCM.
    /// </summary>
    /// <remarks>
    /// This implements message parsing completely independently from the 
    /// MessageParser class. Not sure if that's a good idea or not.
    /// </remarks>
    class MockPcm
    {
        private int state = 0;
        private List<byte> payload = new List<byte>();
        private int crc = 0xFF;

        private byte firstByte;
        private bool physical;
        private byte modeByte;
        private string modeName;
        private byte readWriteBlockId;
        private string readWriteBlockName;
        private string header;
        private string destination;
        private string sender;
        private string crcMessage;
        ILogger logger;

        private byte[] responseBuffer;

        public MockPcm(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Call this before sending a new message.
        /// </summary>
        /// <remarks>
        /// On second thought, this probably shouldn't really be necessary, and might even hide bugs.
        /// </remarks>
        public void ResetCommunications()
        {
            this.crc = 0xFF;
            this.state = 0;
            this.payload.Clear();
        }

        /// <summary>
        /// Send a new message, one byte at a time.
        /// </summary>
        /// <remarks>
        /// One could argue that this makes thing unnecessarily complicated.
        /// However, we already had this code, as part of the VpwDecoder app...
        /// </remarks>
        public void Push(byte value)
        {
            Crc(value);
            switch (state)
            {
                case 0:
                    this.firstByte = value;
                    ParseFirstByte(value);
                    state++;
                    break;

                case 1:
                    this.destination = this.ParseDevice(value);
                    state++;
                    break;

                case 2:
                    this.sender = this.ParseDevice(value);
                    state++;
                    break;

                case 3:
                    this.modeByte = value;

                    if (this.physical)
                    {
                        this.modeName = this.GetPhysicalMode(this.modeByte);
                    }
                    else
                    {
                        this.modeName = this.GetFunctionalMode(this.modeByte);
                    }
                    state++;
                    break;

                case 4:
                    if ((this.modeByte == 0x3B) || (this.modeByte == 0x3C))
                    {
                        this.readWriteBlockId = value;
                        this.readWriteBlockName = this.GetReadWriteBlock(this.readWriteBlockId);
                    }
                    this.payload.Add(value);
                    state++;
                    break;

                default:
                    this.payload.Add(value);
                    break;
            }
        }

        /// <summary>
        /// Call this after sending the last byte of a message.
        /// </summary>
        /// <remarks>
        /// This also shouldn't be necessary. Should be able 
        /// to infer end-of-message from the message itself.
        /// </remarks>
        public void EndOfData()
        {
            if (this.readWriteBlockName != null)
            {
                modeName += " / " + this.readWriteBlockName;
            }

            // TODO: re-order the placeholders and parameters.
            this.logger.AddDebugMessage(
                string.Format(
                    "Mock PCM Received: {4} ({0}), {1,-40} {5,-50} {2}  {3}",
                    this.header, // 0
                    this.sender + " to " + this.destination + ", ", // 1
                    this.crcMessage, // 2
                    string.Empty, // 3
                    this.firstByte.ToString("X2"), // 4
                    this.modeName)); // 5

            if (this.modeByte == 0x27)
            {
                if (this.payload[0] == 0x01)
                {
                    this.responseBuffer = new byte[] { 0x6C, 0xF0, 0x10, 0x67, 0x01, 0x2A, 0xED, 0x03 };
                }
                else if (this.payload[0] == 0x02)
                {
                    // TODO: validate the key.
                    // For now we'll just return a 'success' response every time.
                    this.responseBuffer = new byte[] { 0x6C, 0xF0, 0x10, 0x67, 0x02, 0x34 };
                }
            }
            else if (this.modeByte == 0x3C)
            {
                byte[] responseData = new byte[0];
                switch (this.readWriteBlockId)
                {
                    case BlockId.Vin1:
                        responseData = Encoding.ASCII.GetBytes("12345");
                        break;

                    case BlockId.Vin2:
                        responseData = Encoding.ASCII.GetBytes("ABCDEF");
                        break;

                    case BlockId.Vin3:
                        responseData = Encoding.ASCII.GetBytes("123456");
                        break;

                    case BlockId.Serial1:
                        responseData = Encoding.ASCII.GetBytes("1234");
                        break;

                    case BlockId.Serial2:
                        responseData = Encoding.ASCII.GetBytes("2345");
                        break;

                    case BlockId.Serial3:
                        responseData = Encoding.ASCII.GetBytes("3456");
                        break;

                    case BlockId.BCC:
                        responseData = Encoding.ASCII.GetBytes("4321");
                        break;

                    case BlockId.MEC:
                        responseData = new byte[1];
                        responseData[0] = 123;
                        break;

                    case BlockId.OperatingSystemID:
                        responseData = UnsignedToByteArray(12593358);
                        break;

                    case BlockId.CalibrationID:
                        responseData = UnsignedToByteArray(12345);
                        break;

                    case BlockId.HardwareID:
                        responseData = UnsignedToByteArray(23456);
                        break;

                    case BlockId.SystemCalID:
                        responseData = new byte[] { 0xBA, 0x48, 0xC2, 0x00 };
                        break;
                }

                List<byte> response = new List<byte>();
                response.AddRange(new byte[] { 0x6C, 0xF0, 0x10, 0x7C, this.readWriteBlockId });

                if (this.readWriteBlockId == 1)
                {
                    response.Add(0);
                }

                response.AddRange(responseData);

                this.crc = 0xFF;
                for (int index = 0; index < response.Count; index++)
                {
                    this.Crc(response[index]);
                }

                response.Add(this.GetCrc());

                this.responseBuffer = response.ToArray();
            }
        }

        /// <summary>
        /// Get the response that we queued up after receiving a message. 
        /// Might return a zero-length buffer if no response was prepared.
        /// </summary>
        public byte[] GetResponse()
        {
            byte[] result = this.responseBuffer ?? new byte[0];
            this.responseBuffer = null;
            return result;
        }

        /// <summary>
        /// Interpret the first byte of a VPW message.
        /// </summary>
        private void ParseFirstByte(byte value)
        {
            byte priority = (byte)((value & 0xE0) >> 5);
            string header = (value & 0x10) > 0 ? "GM3" : "GM1";
            string inFrameResponse = (value & 0x08) > 0 ? "No_IFR" : "YesIFR";
            this.physical = (value & 0x04) > 0;
            string addressMode = this.physical ? "Phy" : "Fun";

            this.header = "Pri" + priority.ToString() + " " + header + " " + inFrameResponse + " " + addressMode;
        }

        /// <summary>
        /// Check the CRC byte at the end of a VPW message.
        /// </summary>
        public bool CheckCrc(byte value)
        {
            if (this.GetCrc() == value)
            {
                this.crcMessage = string.Empty; //  "CRC Valid (" + value + "),";
                return true;
            }
            else
            {
                this.crcMessage = string.Format("Actual CRC {0}, expected {1}", value, this.GetCrc());
                return false;
            }
        }

        /// <summary>
        /// Compute the CRC of a series of bytes (use by invoking repeatedly).
        /// </summary>
        private void Crc(byte value)
        {
            crc ^= value;
            int b;
            for (b = 0; b < 8; b++)
            {
                if ((crc & 0x80) != 0)
                {
                    crc <<= 1;
                    crc ^= 0x11D;
                }
                else
                {
                    crc <<= 1;
                }
            }
        }

        /// <summary>
        /// Get the CRC value computed by repeated invocations of the Crc() method above.
        /// </summary>
        private byte GetCrc()
        {
            return (byte)((~this.crc) & 0xFF);
        }

        /// <summary>
        /// Interpret the device-id byte of a VPW message.
        /// </summary>
        private string ParseDevice(byte value)
        {
            if (value < 0x20)
            {
                if (value < 0x10)
                {
                    return "PCM Exp";
                }
                else if (value < 0x18)
                {
                    return "ECU";
                }
                else
                {
                    return "TCU";
                }
            }
            else if (value < 0x28)
            {
                return "Chassis Exp";
            }
            else if (value < 0x30)
            {
                return "Brake";
            }
            else if (value < 0x38)
            {
                return "Steering";
            }
            else if (value < 0x40)
            {
                return "Suspension";
            }
            else if (value < 0x58)
            {
                return "Body Exp";
            }
            else if (value < 0x60)
            {
                return "Restraints";
            }
            else if (value < 0x70)
            {
                return "DriverInfo";
            }
            else if (value < 0x80)
            {
                return "Lighting";
            }
            else if (value < 0x90)
            {
                return "Entertainment";
            }
            else if (value < 0x98)
            {
                return "Personal Communications";
            }
            else if (value < 0xA0)
            {
                return "Climate Control";
            }
            else if (value < 0xC0)
            {
                return "Convenience";
            }
            else if (value < 0xC8)
            {
                return "Security";
            }
            else
            {
                switch (value)
                {
                    case 0xC8:
                        return "Charging";

                    case 0xC9:
                        return "AC to AC";

                    case 0xCA:
                        return "DC to DC";

                    case 0xCB:
                        return "Battery";

                    default:
                        if (value < 0xF0)
                        {
                            return "Undocumented (" + value.ToString("X2") + ")";
                        }
                        else if (value < 0xFE)
                        {
                            return "Scanner/tester";
                        }
                        else if (value == 0xFE)
                        {
                            return "Broadcast";
                        }
                        else if (value == 0xFF)
                        {
                            return "Programmer";
                        }
                        else
                        {
                            return "Undefined";
                        }
                }
            }
        }

        /// <summary>
        /// Interpret the 'mode' byte of a functional VPW message.
        /// </summary>
        private string GetFunctionalMode(byte mode)
        {
            switch (mode)
            {
                case 0x01: return "Request Current Powertrain Diagnostic Data";
                case 0x02: return "Request Powertrain Freeze Frame Data";
                case 0x03: return "Request Powertrain Diagnostic Trouble Codes";
                case 0x04: return "Request to Clear/ Reset Diagnostic Trouble Codes";
                case 0x05: return "Request Oxygen Sensor Monitoring Test Results";
                case 0x06: return "Request On - Board Monitoring Test Results";
                case 0x07: return "Request Pending Powertrain Trouble Codes";
                case 0x08: return "Request Control of On-Board System, Test, or Component";
                case 0x09: return "Request Vehicle Information";
                default: return "Undefined mode: " + mode.ToString("X2");
            }
        }

        /// <summary>
        /// Interpret the 'mode' byte of a physical VPW message.
        /// </summary>
        private string GetPhysicalMode(byte mode)
        {
            switch (mode)
            {
                case 0x10: return "Initiate Diagnostics Operation";
                case 0x11: return "Request Module Reset";
                case 0x12: return "Request Diagnostic Freeze Frame Data";
                case 0x13: return "Request Diagnostic Trouble Code Information";
                case 0x14: return "Clear Diagnostic Information";
                case 0x17: return "Request Status of Diagnostic Trouble Codes";
                case 0x19: return "Request Diagnostic Trouble Codes by Status";
                case 0x20: return "Return to Normal Mode";
                case 0x21: return "Request Diagnostic Data";
                case 0x22: return "Request Diagnostic Data by PID";
                case 0x23: return "Request Diagnostic Data by Memory Address";
                case 0x25: return "Stop Transmitting Requested Data";
                case 0x26: return "Specifiy Data Rates";
                case 0x27: return "Security Access Mode";
                case 0x28: return "Disable Normal Message Transmission";
                case 0x29: return "Enable Normal Message Transmission";
                case 0x2A: return "Request Diagnostic Data Packets";
                case 0x2B: return "Dynamically Define Data Packet by Single Data Offsets";
                case 0x2C: return "Dynamically Define Diagnostic Data Packet";
                case 0x2F: return "Input/Output Control by PID";
                case 0x30: return "Input/Output Control by Data Value ID";
                case 0x31: return "Enter/Start Diagnostic Routine by Test Number";
                case 0x32: return "Exit/Stop Diagnostic Routine by Test Number";
                case 0x33: return "Request Diagnostic Routine Results by Test Number";
                case 0x34: return "Request Download (tool to module)";
                case 0x35: return "Request Upload (module to tool)";
                case 0x36: return "Block Transfer Message";
                case 0x37: return "Request Data Transfer Exit";
                case 0x38: return "Enter Diagnostic Routine by Address";
                case 0x39: return "Exit Diagnostic Routine by Address";
                case 0x3A: return "Request Diagnostic Routine Results";
                case 0x3B: return "Write Data Block";
                case 0x3C: return "Read Data Block";
                case 0x3F: return "Test Device Present";
                case 0x7F: return "General Response Message";
                case 0xA0: return "Request High Speed Mode";
                case 0xA1: return "Begin High Speed Mode";
                case 0xA2: return "Programming Prompt";
                case 0xAE: return "Request Device Control";
                default: return "Undefined mode: " + mode.ToString("X2");
            }
        }

        /// <summary>
        /// Get the name of the read/write block used for the 3B and 3C modes.
        /// </summary>
        /// <remarks>
        /// Based on information found at https://pcmhacking.net/forums/viewtopic.php?f=8&t=5456
        /// </remarks>
        private string GetReadWriteBlock(byte block)
        {
            switch (block)
            {
                case 0x00: return "N/A";
                case 0x01: return "VIN 1 (ASCII)";
                case 0x02: return "VIN 2 (ASCII)";
                case 0x03: return "VIN 3 (ASCII)";
                case 0x04: return "HDW No. (UINT32)";
                case 0x05: return "Serial No 1 (ASCII)";
                case 0x06: return "Serial No 2 (ASCII)";
                case 0x07: return "Serial No 3 (ASCII)";
                case 0x08: return "calibration ID (UINT32)";
                case 0x09: return "N/A";
                case 0x0A: return "4-byte Operating System ID";
                case 0x0B: return "4-byte Engine Calibration ID";
                case 0x0C: return "4-byte Engine Diagnostics ID";
                case 0x0D: return "4-byte Transmission Calibration ID";
                case 0x0E: return "4-byte Transmission Diagnostics ID";
                case 0x0F: return "4-byte Fuel System ID";
                case 0x10: return "4-byte System ID";
                case 0x11: return "4-byte Speedometer ID";
                case 0x12: return "N/A";
                case 0x13: return "N/A";
                case 0x14: return "Broadcast Code (BCC) (ASCII)";
                case 0x41: return "AT Gear 1 Shift Pressure 1";
                case 0x42: return "AT Gear 1 Shift Pressure 2";
                case 0x43: return "AT Gear 1 Shift Pressure 3";
                case 0x44: return "AT Gear 2 Shift Pressure 1";
                case 0x45: return "AT Gear 2 Shift Pressure 2";
                case 0x46: return "AT Gear 2 Shift Pressure 3";
                case 0x47: return "AT Gear 3 Shift Pressure 1";
                case 0x48: return "AT Gear 3 Shift Pressure 2";
                case 0x49: return "AT Gear 3 Shift Pressure 3";
                case 0x4A: return "N/A";
                case 0x4B: return "N/A";
                case 0x4C: return "N/A";
                case 0x4D: return "Unknown";
                case 0x4E: return "Unknown";
                case 0x4F: return "Unknown";
                case 0x6E: return "N/A";
                case 0x72: return "N/A";
                case 0x73: return "0x00";
                case 0x93: return "OS level";
                case 0x94: return "Calibration Level";
                case 0x95: return "Diagnostic Level";
                case 0x96: return "Transmission Calibration Level";
                case 0x97: return "Transmission Diagnostic Level";
                case 0x98: return "Fuel System Level";
                case 0x99: return "Vehicle System Level";
                case 0x9A: return "Speedometer Calibration Level";
                case 0xA0: return "Manufacturer Enable Counter";
                default: return "Unknown";
            }
        }

        /// <summary>
        /// Convert an unsigned 4-byte value to a byte array.
        /// </summary>
        private byte[] UnsignedToByteArray(uint value)
        {
            byte[] native = BitConverter.GetBytes(value);
            byte[] result = new byte[4];
            result[0] = native[3];
            result[1] = native[2];
            result[2] = native[1];
            result[3] = native[0];
            return result;
        }
    }
}
