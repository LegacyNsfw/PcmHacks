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
            byte[] Bytes = new byte[] { Priority.Physical0, DeviceId.Pcm, DeviceId.Tool, Mode.ConfigureDynamicData, position, offset >> 8, offset, 0xFF, 0xFF };
            return new Message(Bytes);
        }

        /// <summary>
        /// Create a request to read the PCM's operating system ID.
        /// </summary>
        /// <returns></returns>
        public Message BeginLogging(params byte dpid)
        {
            // That final 0x01 might need to be the length of the dpid array?
            byte[] bytes = new byte[] { Priority.Physical0, DeviceId.Pcm, DeviceId.Tool, Mode.SendDynamicData, 0x01 };
            return new Message(bytes.Concat(dpid));
        }

        /// <summary>
        /// Create a request to read the PCM's Calibration ID.
        /// </summary>
        /// <returns></returns>
        public Message CreateCalibrationIdReadRequest()
        {
            return CreateReadRequest(BlockId.CalibrationID);
        }

        /// <summary>
        /// Create a request to read the PCM's Hardware ID.
        /// </summary>
        /// <returns></returns>
        public Message CreateHardwareIdReadRequest()
        {
            return CreateReadRequest(BlockId.HardwareID);
        }

        /// <summary>
        /// Parse a 32-bit value from the first four bytes of a message payload.
        /// </summary>
        public Response<UInt32> ParseUInt32(Message message, byte responseMode)
        {
            byte[] bytes = message.GetBytes();
            int result = 0;
            ResponseStatus status;

            byte[] expected = new byte[] { 0x6C, DeviceId.Tool, DeviceId.Pcm, responseMode };
            if (!TryVerifyInitialBytes(bytes, expected, out status))
            {
                return Response.Create(ResponseStatus.Error, (UInt32)result);
            }

            result = bytes[5] << 24;
            result += bytes[6] << 16;
            result += bytes[7] << 8;
            result += bytes[8];

            return Response.Create(ResponseStatus.Success, (UInt32)result);
        }


    }
}
