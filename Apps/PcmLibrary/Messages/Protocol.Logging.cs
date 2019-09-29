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

    public enum DefineBy
    {
        Offset = 0,
        Pid = 1,
        Address = 2,
        Proprietary = 3,
    }

    public class RawLogData
    {
        public byte Dpid { get; private set; }
        public byte[] Payload { get; private set; }

        public RawLogData(byte dpid, byte[] payload)
        {
            this.Dpid = dpid;
            this.Payload = payload;
        }
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

        public Message ConfigureDynamicData(byte dpid, DefineBy defineBy, int offset, int size, UInt32 id)
        {
            int combined = (((int)defineBy) << 6) | (offset << 3) | size;
            byte byte1, byte2, byte3;

            switch (defineBy)
            {
                case DefineBy.Offset:
                    byte1 = (byte)id;
                    byte2 = 0xFF;
                    byte3 = 0xFF;
                    break;

                case DefineBy.Pid:
                    byte1 = (byte)(id >> 8);
                    byte2 = (byte)id;
                    byte3 = 0xFF;
                    break;

                case DefineBy.Address:
                    byte1 = (byte)(id >> 16);
                    byte2 = (byte)(id >> 8);
                    byte3 = (byte)id;
                    break;

                default:
                    throw new InvalidOperationException("Unsupported DefineBy value: " + defineBy.ToString());
            }

            byte[] payload = new byte[]
            {
                Priority.Physical0,
                DeviceId.Pcm,
                DeviceId.Tool,
                Mode.ConfigureDynamicData,
                dpid,
                (byte)combined,
                byte1,
                byte2,
                byte3,
                0xFF
            };

            return new Message(payload);
        }

        /// <summary>
        /// Create a request to read the PCM's operating system ID.
        /// </summary>
        /// <returns></returns>
        public Message BeginLogging(params byte[] dpid)
        {
            // ResponseType values:
            // 0x01 = send once
            // 0x12 = send slowly
            // 0x13 = send medium
            // 0x24 = send fast
            byte responseType = 0x24;
            byte[] header = new byte[] { Priority.Physical0, DeviceId.Pcm, DeviceId.Tool, Mode.SendDynamicData, responseType };
            byte[] padding = new byte[4];// { 0xFF, 0xFF, 0xFF, 0xFF };
            IEnumerable<byte> test = padding.Take(5 - dpid.Length);
            return new Message(header.Concat(dpid).Concat(test).ToArray());
        }

        // TODO: create a Parse method for the 6C
        public bool TryParseRawLogData(Message message, out RawLogData rawLogData)
        {
            ResponseStatus unused;
            if (!TryVerifyInitialBytes(message.GetBytes(), new byte[] { 0x8C, DeviceId.Tool, DeviceId.Pcm, 0x6A }, out unused))
            {
                rawLogData = null;
                return false;
            }

            rawLogData = new RawLogData(message[4], message.GetBytes().Skip(5).Take(6).ToArray());
            return true;
        }
    }
}
