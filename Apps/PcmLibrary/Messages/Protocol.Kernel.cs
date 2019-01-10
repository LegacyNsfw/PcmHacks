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
        /// Create a request to get the operating system ID from the kernel.
        /// </summary>
        /// <remarks>
        /// It was tempting to just implement the same OS ID query as the PCM software,
        /// but then the app would need to do a kernel request of some type to determine 
        /// whether the PCM is running the GM OS or the kernel. However, in almost all 
        /// cases, the PCM will be running the GM OS, and that kernel request would slow
        /// down the most common usage scenario.
        ///
        /// So we keep the common scenario fast by having the standard OS ID request 
        /// succeed only when the standard operating system is running. When it fails,
        /// that puts the app into a slower path were it checks for a kernel and then 
        /// asks the kernel what OS is installed. Or it checks for recovery mode, loads
        /// the kernel, and then asks the kernel what OS is installed.
        /// </remarks>
        public Message CreateOperatingSystemIdKernelRequest()
        {
            return new Message(new byte[] { 0x6C, 0x10, 0xF0, 0x3D, 0x03 });
        }

        /// <summary>
        /// Parse the operating system ID from a kernel response.
        /// </summary>
        public Response<UInt32> ParseOperatingSystemIdKernelResponse(Message message)
        {
            return ParseUInt32(message, 0x7D);
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
