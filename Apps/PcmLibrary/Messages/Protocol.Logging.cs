using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PcmHacking
{
    public enum DefineBy
    {
        Offset = 0,
        Pid = 1,
        Address = 2,
        Proprietary = 3,
    }

    /// <summary>
    /// Contains the raw data returned from the PCM for a dpid.
    /// </summary>
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

    /// <summary>
    /// A collection of dpids, used to bridge the gap between configuring dpids and requesting dpids.
    /// </summary>
    public class DpidCollection
    {
        public byte[] Values { get; private set; }

        public DpidCollection(byte[] dpids)
        {
            this.Values = dpids;
        }
    }

    public partial class Protocol
    {
        /// <summary>
        /// Create a request message that will configure a single value to include in a given dpid.
        /// </summary>
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
        /// Create a request to read data from the PCM
        /// </summary>
        public Message RequestDpids(DpidCollection dpids)
        {
#if FAST_LOGGING
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
#else
            byte[] header = new byte[] { Priority.Physical0, DeviceId.Pcm, DeviceId.Tool, Mode.SendDynamicData, 0x01 };
            return new Message(header.Concat(dpids.Values).ToArray());
#endif
        }

        /// <summary>
        /// Extract the raw data from a dpid response message.
        /// </summary>
        public bool TryParseRawLogData(Message message, out RawLogData rawLogData)
        {
            ResponseStatus unused;
            if (!TryVerifyInitialBytes(message.GetBytes(), new byte[] { 0x6C, DeviceId.Tool, DeviceId.Pcm, 0x6A }, out unused))
            {
                rawLogData = null;
                return false;
            }

            rawLogData = new RawLogData(message[4], message.GetBytes().Skip(5).Take(6).ToArray());
            return true;
        }

        /// <summary>
        /// Create a request for a single PID.
        /// </summary>
        public Message CreatePidRequest(UInt32 pid)
        {
            Message request;

            /* Using OBD2 "Functional" addressing - doesn't work well with the rest of the code.
            if (pid <= 0xFFFF)
            {
                request = new Message(new byte[] { 0x68, 0x6A, 0xF1, (byte)(pid >> 8), (byte)pid });
            }
            else if (pid <= 0xFFFFFF)
            {
                request = new Message(new byte[] { 0x68, 0x6A, 0xF1, (byte)(pid >> 16), (byte)(pid >> 8), (byte)pid });
            }
            else
            {
                request = new Message(new byte[] { 0x68, 0x6A, 0xF1, (byte)(pid >> 24), (byte)(pid >> 16), (byte)(pid >> 8), (byte)pid });
            }
            */

            // Using OBD2 "Physical" addressing.
            request = new Message(new byte[] { Priority.Physical0, DeviceId.Pcm, DeviceId.Tool, 0x22, (byte)(pid >> 8), (byte)pid, 0x01 });

            return request;
        }

        /// <summary>
        /// Parse the value of the requested PID.
        /// </summary>
        public Response<int> ParsePidResponse(Message message)
        {
            ResponseStatus status;
            if (!this.TryVerifyInitialBytes(
                message.GetBytes(), 
                new byte[] { 0x6C, DeviceId.Tool, DeviceId.Pcm, 0x62 }, out status))
            {
                return Response.Create(status, 0);    
            }

            int value;
            switch (message.Length)
            {
                case 7:
                    value = message[6];
                    break;

                case 8:
                    value = message[6];
                    value <<= 8;
                    value |= message[7];
                    break;

                default:
                    throw new UnsupportedFormatException("Only 1 and 2 byte PIDs are supported for now.");
            }

            return Response.Create(ResponseStatus.Success, value);
        }
    }
}
