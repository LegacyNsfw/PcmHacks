using System;
using System.Collections.Generic;
using System.Text;

namespace PcmHacking
{
    /// <summary>
    /// Mode 3D was apparently not used for anything, so it's being taken
    /// for communications with the kernel.
    /// </summary>
    public partial class Protocol
    {
        /// <summary>
        /// Create a request for the kernel version.
        /// </summary>
        public Message CreateKernelVersionQuery()
        {
            return new Message(new byte[] { 0x6C, 0x10, 0xF0, 0x3D, 0x00 });
        }

        internal Response<UInt32> ParseKernelVersion(Message responseMessage)
        {
            return ParseUInt32(responseMessage, 0x3D, 0x00);
        }

        /// <summary>
        /// Create a request to identify the flash chip. 
        /// </summary>
        public Message CreateFlashMemoryTypeQuery()
        {
            return new Message(new byte[] { 0x6C, 0x10, 0xF0, 0x3D, 0x01 });
        }

        internal Response<UInt32> ParseFlashMemoryType(Message responseMessage)
        {
            return ParseUInt32(responseMessage, 0x3D, 0x01);
        }

        /// <summary>
        /// Create a request to get the CRC of a byte range.
        /// </summary>
        public Message CreateCrcQuery(UInt32 address, UInt32 size)
        {
            byte[] requestBytes = new byte[] { 0x6C, 0x10, 0xF0, 0x3D, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            requestBytes[5] = unchecked((byte)(size >> 16));
            requestBytes[6] = unchecked((byte)(size >> 8));
            requestBytes[7] = unchecked((byte)size);
            requestBytes[8] = unchecked((byte)(address >> 16));
            requestBytes[9] = unchecked((byte)(address >> 8));
            requestBytes[10] = unchecked((byte)address);
            return new Message(requestBytes);
        }

        /// <summary>
        /// Parse the response to a CRC query.
        /// </summary>
        internal Response<UInt32> ParseCrc(Message responseMessage, UInt32 address, UInt32 size)
        {
            ResponseStatus status;
            byte[] expected = new byte[]
            {
                0x6C,
                DeviceId.Tool,
                DeviceId.Pcm,
                0x7D,
                0x02,
                unchecked((byte)(size >> 16)),
                unchecked((byte)(size >> 8)),
                unchecked((byte)size),
                unchecked((byte)(address >> 16)),
                unchecked((byte)(address >> 8)),
                unchecked((byte)address),
            };

            if (!TryVerifyInitialBytes(responseMessage, expected, out status))
            {
                byte[] refused = { 0x6C, DeviceId.Tool, DeviceId.Pcm, 0x7F, 0x3D, 0x02 };
                if (TryVerifyInitialBytes(responseMessage, refused, out status))
                {
                    return Response.Create(ResponseStatus.Refused, (UInt32)0);
                }

                return Response.Create(status, (UInt32)0);
            }

            byte[] responseBytes = responseMessage.GetBytes();
            if (responseBytes.Length < 15)
            {
                return Response.Create(ResponseStatus.Truncated, (UInt32)0);
            }

            int crc =
                (responseBytes[11] << 24) |
                (responseBytes[12] << 16) |
                (responseBytes[13] << 8) |
                responseBytes[14];

            return Response.Create(ResponseStatus.Success, (UInt32)crc);
        }

        /// <summary>
        /// Ask the kernel to erase a block of flash memory.
        /// </summary>
        public Message CreateFlashEraseBlockRequest(UInt32 baseAddress)
        {
            return new Message(new byte[]
            {
                0x6C,
                0x10,
                0xF0,
                0x3D,
                0x05,
                (byte)(baseAddress >> 16),
                (byte)(baseAddress >> 8),
                (byte)baseAddress
            });
        }

        /// <summary>
        /// Find out whether an erase request was successful.
        /// </summary>
        internal Response<byte> ParseFlashEraseBlock(Message message)
        {
            return ParseByte(message, 0x3D, 0x05);
        }

        /// <summary>
        /// Create a request for implementation details... for development use only.
        /// </summary>
        public Message CreateDebugQuery()
        {
            return new Message(new byte[] { 0x6C, 0x10, 0xF0, 0x3D, 0xFF });
        }

        /// <summary>
        /// Create a message to tell the RAM-resident kernel to exit.
        /// </summary>
        public Message CreateExitKernel()
        {
            byte[] bytes = new byte[] { Priority.Physical0, DeviceId.Pcm, DeviceId.Tool, 0x20 };
            return new Message(bytes);
        }
    }
}
