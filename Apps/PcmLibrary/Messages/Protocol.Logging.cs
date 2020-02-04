//#define FAST_LOGGING

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
            // 0x14 = send medium
            // 0x24 = send fast
            byte responseType = 0x01;
            byte[] header = new byte[] { Priority.Physical0, DeviceId.Pcm, DeviceId.Tool, Mode.SendDynamicData, responseType };
            byte[] padding = new byte[4];// { 0xFF, 0xFF, 0xFF, 0xFF };
            IEnumerable<byte> test = padding.Take(5 - dpids.Values.Length);
            return new Message(header.Concat(dpids.Values).Concat(test).ToArray());
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
        public Message CreatePidRequest(byte deviceId, UInt32 pid)
        {
            Message request;

            // Using OBD2 "Physical" addressing - doesn't work with EBCM.
            request = new Message(new byte[] { Priority.Physical0, deviceId, DeviceId.Tool, 0x22, 0x00, (byte)pid, 0x01 });

            // Try to request a "static DPID" ? - no luck with this either.
            //request = new Message(new byte[] { Priority.Physical0, DeviceId.Ebcm, DeviceId.Tool, Mode.SendDynamicData, 0x01, 0x01 });

            // Get all diagnostic trouble codes - this actually works with the EBCM!
            //request = new Message(Priority.Physical0, DeviceId.Ebcm, DeviceId.Tool, 0x19, 0xFF, 0xFF, 0x00);

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

                default:
                    value = message[6];
                    value <<= 8;
                    value |= message[7];
                    break;
            }

            return Response.Create(ResponseStatus.Success, value);
        }


        /// <summary>
        /// Parse the value of the requested PID.
        /// </summary>
        public Response<byte[]> ParsePidResponse2(Message message)
        {
            ResponseStatus status;
            if (!this.TryVerifyInitialBytes(
                message.GetBytes(),
                new byte[] { 0x6C, DeviceId.Tool, DeviceId.Ebcm, 0x62 }, out status))
            {
                return Response.Create(status, new byte[0]);
            }

            byte[] result = new byte[message.Length - 6];
            for(int index = 6; index < message.Length; index++)
            {
                result[index - 6] = message[6];
            }
            
            return Response.Create(ResponseStatus.Success, result);
        }
    }
}
