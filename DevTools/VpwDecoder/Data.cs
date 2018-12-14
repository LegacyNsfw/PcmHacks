using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VpwDecoder
{
    class Data
    {
        /// <summary>
        /// Returns a string describing the category of a given device id
        /// </summary>
        public static string DecodeDevice(byte DeviceId)
        {
            if (DeviceId >= 0x00 && DeviceId <= 0x0F) return "Powertrain module";
            if (DeviceId >= 0x10 && DeviceId <= 0x17) return "Engine module (PCM)";
            if (DeviceId >= 0x18 && DeviceId <= 0x1F) return "Transmission module";
            if (DeviceId >= 0x20 && DeviceId <= 0x27) return "Chassis module";
            if (DeviceId >= 0x28 && DeviceId <= 0x2F) return "Brake module";
            if (DeviceId >= 0x30 && DeviceId <= 0x37) return "Steering module";
            if (DeviceId >= 0x38 && DeviceId <= 0x3F) return "Suspension module";
            if (DeviceId >= 0x40 && DeviceId <= 0x47) return "Body module";
            if (DeviceId >= 0x48 && DeviceId <= 0x5F) return "Restraint module";
            if (DeviceId >= 0x60 && DeviceId <= 0x6F) return "Driver info display";
            if (DeviceId >= 0x70 && DeviceId <= 0x7F) return "Lighting module";
            if (DeviceId >= 0x80 && DeviceId <= 0x8F) return "Entertainment";
            if (DeviceId >= 0x90 && DeviceId <= 0x97) return "Personal comm.";
            if (DeviceId >= 0x98 && DeviceId <= 0x9F) return "Climate control";
            if (DeviceId >= 0xA0 && DeviceId <= 0xBF) return "Convenience";
            if (DeviceId >= 0xC0 && DeviceId <= 0xC7) return "Security module";
            if (DeviceId >= 0xC8 && DeviceId <= 0xCB) return "EV energy system";
            if (DeviceId == 0xC8) return "Utility connection";
            if (DeviceId == 0xC9) return "AC to AC conversion";
            if (DeviceId == 0xCA) return "AC to DC conversion";
            if (DeviceId == 0xCB) return "Energy storage unit";
            //if (DeviceId >= 0xCC && DeviceId <= 0xCF) return "future expansion";
            //if (DeviceId >= 0xD0 && DeviceId <= 0xEF) return "manufacturer specific";
            if (DeviceId >= 0xF0 && DeviceId <= 0xFD) return "External tool";
            if (DeviceId == 0xFE) return "Broadcast message";

            return "unknown";
        }

        public static string DecodePriority(byte priority)
        {
            string description;
            switch(priority)
            {
                case 0x6D:
                    description = "Block";
                    break;

                case 0x6A:
                    description = "Type Two";
                    break;

                case 0x8C:
                    description = "Diagnostic";
                    break;

                default:
                    string priorityLevel = (priority >> 5).ToString();
                    string addressing = ((priority & 4) == 0) ? "Functional" : "Physical  ";
                    description = string.Format("{0,10} {1}", addressing, priorityLevel);
                    break;
            }

            /* based on information from http://www.fastfieros.com/tech/vpw_communication_protocol.htm
            Bits 7,6 and 5 are priority 0=High, 7=Low
            Bit 4 is header style (0=3 byte header-GM, 1=1 byte header-??)

            Bit 3 is In Frame Response (0=Ford, 1=GM)
            Bit 2 is addressing mode (1=Physical, 0=Functional)
            Bit 1,0 is message type: (depending on bit 2 and 3 see below)
            */

            return string.Format("{0,-12}", description);
        }

        // Decode the Mode and Submode
        // Mostly based on information from this page:
        // http://www.fastfieros.com/tech/vpw_communication_protocol.htm
        public static string DecodeMode(byte priorityByte, byte mode, byte submode, bool haveSubmode)
        {
            string ack = string.Empty;
            byte switchMode = mode;

            if ((mode & 0x40) != 0)
            {
                ack = " (ack)";
                switchMode = (byte)(mode - 0x40);
            }

            string modeString = string.Empty;
            string submodeString = string.Empty;

            // Special-case for tool-present messages.
            if ((priorityByte == 0x8C) && (mode == 0x3F))
            {
                modeString = "Tool Present";

                // The 'ack' thing is hacky...
                ack = string.Empty;
            }
            else if (mode == 0x7F)
            {
                modeString = "Reject";
                switchMode = submode;
                string rejectedMode = string.Empty;
                string unused = string.Empty;
                GetMode(submode, 0, ref rejectedMode, ref unused);
                modeString = rejectedMode;
                submodeString = "REJECTED";
            }
            else
            {
                GetMode(switchMode, submode, ref modeString, ref submodeString);
            }

            modeString = modeString + ack;

            if (haveSubmode)
            {
                return string.Format("{0:X2} {1, -22} {2:X2} {3,-8}", mode, modeString, submode, submodeString);
            }

            return string.Format("{0:X2} {1, -22}    {2,-8}", mode, modeString, string.Empty);
        }


        private static void GetMode(byte mode, byte submode, ref string modeString, ref string submodeString)
        {
            switch (mode)
            {
                case 0x75:
                    modeString = "Reject";
                    break;

                case 0x01:
                    modeString = "Req Diag Data";
                    break;

                case 0x02:
                    modeString = "Req Frz Frm";
                    break;

                case 0x03:
                    modeString = "Req DTCs";
                    break;

                case 0x04:
                    modeString = "Clear DTCs";
                    break;

                case 0x05:
                    modeString = "Req O2 Result";
                    break;

                case 0x06:
                    modeString = "Req Mon. Result";
                    break;

                case 0x07:
                    modeString = "Req Pending Res";
                    break;

                case 0x08:
                    modeString = "Request Control";
                    break;

                case 0x09:
                    modeString = "Req. Veh. Info";
                    break;

                case 0x10:
                    modeString = "Begin Diagnostic";
                    break;

                case 0x11:
                    modeString = "Req Module Reset";
                    break;

                case 0x12:
                    modeString = "Req Frz Frm";
                    break;

                case 0x13:
                    modeString = "Req DTCs";
                    break;

                case 0x14:
                    modeString = "Clear DTCs";
                    break;

                case 0x17:
                    modeString = "Req DTC Status";
                    break;

                case 0x19:
                    modeString = "Req DTC @ Status";
                    break;

                case 0x20:
                    modeString = "Return To Normal";
                    break;

                case 0x21:
                    modeString = "Req Diag Data";
                    break;

                case 0x22:
                    modeString = "Req Data by PID";
                    break;

                case 0x23:
                    modeString = "Req Data by mem";
                    break;

                case 0x24:
                    modeString = "Req Scale/Offset";
                    break;

                case 0x25:
                    modeString = "Cancel Request";
                    break;

                case 0x26:
                    modeString = "Req Data Rates";
                    break;

                case 0x27:
                    modeString = "Security";
                    submodeString = ((submode == 1) ? "GetSeed" : ((submode == 2) ? "SendKey" : submodeString));
                    break;

                case 0x28:
                    modeString = "Suppress Chatter";
                    break;

                case 0x29:
                    modeString = "Reenable Chatter";
                    break;

                case 0x2A:
                    modeString = "Req Diag Packets";
                    break;

                case 0x2B:
                    // Dynamically define data packet by single data offset (WTF?)
                    modeString = "DynDef Offset";
                    break;

                case 0x2C:
                    // Dynamically define diagnostic data packet
                    modeString = "DynDef Diag Pkt";
                    break;

                case 0x2F:
                    // Input / Output control by PID
                    modeString = "IO by PID";
                    break;

                case 0x30:
                    // Input / output control by data value ID
                    modeString = "IO by Data ID";
                    break;

                case 0x31:
                    modeString = "Start Diagnostic";
                    break;

                case 0x32:
                    modeString = "Stop Diagnostic";
                    break;

                case 0x33:
                    modeString = "Req Diag Result";
                    break;

                case 0x34:
                    // Tool to module
                    modeString = "Req Download";
                    break;

                case 0x35:
                    // Module to tool
                    modeString = "Req Upload";
                    break;

                case 0x36:
                    submodeString = ((submode == 0) ? "Transfer" : ((submode == 0x80) ? "Execute" : submodeString));
                    modeString = "Block Transfer";
                    break;

                case 0x37:
                    modeString = "Halt Transfer";
                    break;

                case 0x38:
                    modeString = "Start Diag @Addr";
                    break;

                case 0x39:
                    modeString = "Stop Diag @Addr";
                    break;

                case 0x3A:
                    modeString = "Req Diag Result";
                    break;

                case 0x3B:
                    modeString = "Write Block";
                    break;

                case 0x3C:
                    modeString = "Read Block";
                    break;

                case 0x3F:
                    modeString = "Device Present";
                    break;

                case 0xA0:
                    modeString = "4X Prepare";
                    break;

                case 0xA1:
                    modeString = "4X Switch";
                    break;

                case 0xA2:
                    // Programming Prompt
                    modeString = "Programming";
                    break;

                case 0xAE:
                    modeString = "Req Dev Ctrl";
                    break;

                default:
                    break;
            }
        }
    }
}
