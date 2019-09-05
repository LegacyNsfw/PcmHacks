using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PcmHacking
{
    public enum DpidPosition
    {
        Position12 = 0x4A,
        Position34 = 0x5A,
        Position56 = 0x6A, // ?
        Position1 = 0x49,
        Position2 = 0x51,
        Position3 = 0x59,
        Position4 = 0x61,
        Position5 = 0x69,
        Position6 = 0x71,
    }
    public partial class Protocol
    {
        /// <summary>
        /// Create a request to read the given block of PCM memory.
        /// </summary>
        public Message ConfigureDynamicData(byte dpid, DpidPosition position, UInt16 pid)
        {
            byte[] header = new byte[]
            {
                Priority.Physical0,
                DeviceId.Pcm,
                DeviceId.Tool,
                Mode.ConfigureDynamicData,
                dpid,
                (byte)position,
                (byte)(pid >> 8),
                (byte)pid,
                0xFF,
                0xFF
            };
            return new Message(header);
        }

        /// <summary>
        /// Create a request to read the PCM's operating system ID.
        /// </summary>
        /// <returns></returns>
        public Message BeginLogging(params byte[] dpid)
        {
            // That final 0x01 might need to be the length of the dpid array?
            byte[] header = new byte[] { Priority.Physical0, DeviceId.Pcm, DeviceId.Tool, Mode.SendDynamicData, 0x01 };
            return new Message(header.Concat(dpid).ToArray());
        }
    }
}
