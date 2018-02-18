using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VpwDecoder
{
    class Parser : IDisposable
    {
        private int state = 0;
        private List<byte> payload = new List<byte>();
        private string fileNameBase;
        private int lineNumber;

        private byte firstByte;
        private bool physical;
        private byte modeByte;
        private string modeName;
        private string header;
        private string destination;
        private string sender;
        private string crcMessage;

        public Parser(string fileName, int lineNumber)
        {
            this.fileNameBase = fileName;
            this.lineNumber = lineNumber;
        }

        public void Dispose()
        {
            string message;
            if (payload.Count < 10)
            {
                message = string.Join(" ", payload.Select(x => x.ToString("X2")));
            }
            else
            {
                string fileName = fileNameBase + "-Line-" + this.lineNumber.ToString() + ".bin";
                message = "Payload saved to " + fileName;
                using (Stream output = File.OpenWrite(fileName))
                {
                    foreach (byte b in this.payload)
                        output.WriteByte(b);
                }
            }


            Console.WriteLine(
                string.Format(
                    "{4} ({0}), {1,-30} {2} {5,-40} {3}",
                    this.header,
                    this.sender + " to " + this.destination + ", ",
                    this.crcMessage,
                    message,
                    this.firstByte.ToString("X2"),
                    this.modeName));
        }

        public void Push(string hex, byte value)
        {
            Crc(value);
            switch(state)
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

                default:
                    this.payload.Add(value);
                    break;
            }
        }

        private void ParseFirstByte(byte value)
        {
            byte priority = (byte) ((value & 0xE0) >> 5);
            string header = (value & 0x10) > 0 ? "GM3" : "GM1";
            string inFrameResponse = (value & 0x08) > 0 ? "No_IFR" : "YesIFR";
            this.physical = (value & 0x04) > 0;
            string addressMode = this.physical ? "Phy" : "Fun";

            this.header = "Pri" + priority.ToString() + " " + header + " " + inFrameResponse + " " + addressMode;
        }

        public void CheckCrc(byte value)
        {
            if(this.GetCrc() == value)
            {
                this.crcMessage = string.Empty; //  "CRC Valid (" + value + "),";
            }
            else
            {
                this.crcMessage = string.Format("Actual CRC {0}, expected {1}", value, this.GetCrc());
            }
        }

        private int crc = 0xFF;
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

        private byte GetCrc()
        {
            return (byte)((~this.crc) & 0xFF);
        }

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
                switch(value)
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
                            return "Undocumented";
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

        private string GetPhysicalMode(byte mode)
        {
            switch(mode)
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
    }
}
