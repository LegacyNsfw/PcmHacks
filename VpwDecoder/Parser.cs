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
                    "{4} ({0}), {1,-30} {2} {3}",
                    this.header,
                    this.sender + " to " + this.destination + ", ",
                    this.crcMessage,
                    message,
                    this.firstByte.ToString("X2")));
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
            string addressMode = (value & 0x04) > 0 ? "Phy" : "Fun";

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
        /*
        private int crc8(Frame_T frame)
        {
            unsigned t_crc;
            int f, b;

            t_crc = 0xFF;
            for (f = 0; f < frame.length; f++ ) 
            {
                t_crc ^= frame.data[f];
                for (b = 0; b < 8 ; b++ )
                {
                    if ((t_crc & 0x80) != 0)
                    {
                        t_crc <<= 1;
                        t_crc ^= 0x11D;
                    }
                    else
                    {
                        t_crc <<= 1;
                    }
                }
            }
            return (~t_crc) & 0xFF;
        }
        */

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
    }
}
