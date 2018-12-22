using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// This class is responsible for parsing messages that are received from the PCM.
    /// </summary>
    /// <remarks>
    /// This class requires byte arrays that are byte-for-byte identical to what was
    /// sent by the PCM, but without the CRC byte. If the currently selected hardware
    /// device sends a CRC byte to the caller, the Device class for that hardware must
    /// remove the CRC byte before the byte array is passed to this class.
    /// </remarks>
    public class MessageParser
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// Doesn't do much. In theory we could use static methods for all of this, but
        /// I'm allergic to static classes. They make testing harder.
        /// </remarks>
        public MessageParser()
        {
        }

        /// <summary>
        /// Parse the response to an OS ID request.
        /// </summary>
        public Response<UInt32> ParseBlockUInt32(Message message)
        {
            byte[] bytes = message.GetBytes();
            int result = 0;
            ResponseStatus status;

            byte[] expected = new byte[] { 0x6C, DeviceId.Tool, DeviceId.Pcm, 0x7C };
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

        /// <summary>
        /// Parse the responses to the three requests for VIN information.
        /// </summary>
        public Response<string> ParseVinResponses(byte[] response1, byte[] response2, byte[] response3)
        {
            string result = "Unknown";
            ResponseStatus status;

            byte[] expected = new byte[] { 0x6C, DeviceId.Tool, DeviceId.Pcm, 0x7C, BlockId.Vin1 };
            if (!TryVerifyInitialBytes(response1, expected, out status))
            {
                return Response.Create(status, result);
            }

            expected = new byte[] { 0x6C, DeviceId.Tool, DeviceId.Pcm, 0x7C, BlockId.Vin2 };
            if (!TryVerifyInitialBytes(response2, expected, out status))
            {
                return Response.Create(status, result);
            }

            expected = new byte[] { 0x6C, DeviceId.Tool, DeviceId.Pcm, 0x7C, BlockId.Vin3 };
            if (!TryVerifyInitialBytes(response3, expected, out status))
            {
                return Response.Create(status, result);
            }

            byte[] vinBytes = new byte[17];
            Buffer.BlockCopy(response1, 6, vinBytes, 0, 5);
            Buffer.BlockCopy(response2, 5, vinBytes, 5, 6);
            Buffer.BlockCopy(response3, 5, vinBytes, 11, 6);
            string vin = System.Text.Encoding.ASCII.GetString(vinBytes);
            return Response.Create(ResponseStatus.Success, vin);
        }
        
        /// <summary>
        /// Parse the responses to the three requests for Serial Number information.
        /// </summary>
        public Response<string> ParseSerialResponses(Message response1, Message response2, Message response3)
        {
            string result = "Unknown";
            ResponseStatus status;

            byte[] expected = new byte[] { 0x6C, DeviceId.Tool, DeviceId.Pcm, 0x7C, BlockId.Serial1 };
            if (!TryVerifyInitialBytes(response1, expected, out status))
            {
                return Response.Create(status, result);
            }

            expected = new byte[] { 0x6C, DeviceId.Tool, DeviceId.Pcm, 0x7C, BlockId.Serial2 };
            if (!TryVerifyInitialBytes(response2, expected, out status))
            {
                return Response.Create(status, result);
            }

            expected = new byte[] { 0x6C, DeviceId.Tool, DeviceId.Pcm, 0x7C, BlockId.Serial3 };
            if (!TryVerifyInitialBytes(response3, expected, out status))
            {
                return Response.Create(status, result);
            }

            byte[] serialBytes = new byte[12];
            Buffer.BlockCopy(response1.GetBytes(), 5, serialBytes, 0, 4);
            Buffer.BlockCopy(response2.GetBytes(), 5, serialBytes, 4, 4);
            Buffer.BlockCopy(response3.GetBytes(), 5, serialBytes, 8, 4);

            byte[] printableBytes = Utility.GetPrintable(serialBytes);
            string serial = System.Text.Encoding.ASCII.GetString(printableBytes);

            return Response.Create(ResponseStatus.Success, serial);
        }

        public Response<string> ParseBCCresponse(Message responseMessage)
        {
            string result = "Unknown";
            ResponseStatus status;
            byte[] response = responseMessage.GetBytes();

            byte[] expected = new byte[] { 0x6C, DeviceId.Tool, DeviceId.Pcm, 0x7C, BlockId.BCC };
            if (!TryVerifyInitialBytes(response, expected, out status))
            {
                return Response.Create(status, result);
            }

            byte[] BCCBytes = new byte[4];
            Buffer.BlockCopy(response, 5, BCCBytes, 0, 4);

            byte[] printableBytes = Utility.GetPrintable(BCCBytes);
            string BCC  = System.Text.Encoding.ASCII.GetString(printableBytes);
            
            return Response.Create(ResponseStatus.Success, BCC);
        }

        public Response<string> ParseMECresponse(Message responseMessage)
        {
            string result = "Unknown";
            ResponseStatus status;
            byte[] response = responseMessage.GetBytes();

            byte[] expected = new byte[] { 0x6C, DeviceId.Tool, DeviceId.Pcm, 0x7C, BlockId.MEC };
            if (!TryVerifyInitialBytes(response, expected, out status))
            {
                return Response.Create(status, result);
            }

            string MEC = response[5].ToString();
            
            return Response.Create(ResponseStatus.Success, MEC);
        }

        /// <summary>
        /// Indicates whether or not the reponse indicates that the PCM is unlocked.
        /// </summary>
        public bool IsUnlocked(byte[] response)
        {
            ResponseStatus status;
            byte[] unlocked = { Priority.Physical0, DeviceId.Tool, DeviceId.Pcm, 0x67, 0x01, 0x37 };

            if (TryVerifyInitialBytes(response, unlocked, out status))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parse the response to a seed request.
        /// </summary>
        public Response<UInt16> ParseSeed(byte[] response)
        {
            ResponseStatus status;
            UInt16 result = 0;

            byte[] unlocked = { Priority.Physical0, 0x70, DeviceId.Pcm, Mode.Seed + Mode.Response, 0x01, 0x37 };
            byte[] seed = new byte[] { Priority.Physical0, DeviceId.Tool, DeviceId.Pcm, 0x67, 0x01, };

            if (TryVerifyInitialBytes(response, unlocked, out status))
            {
                status = ResponseStatus.Success;
                return Response.Create(ResponseStatus.Success, result);
            }

            if (!TryVerifyInitialBytes(response, seed, out status))
            {
                return Response.Create(ResponseStatus.Error, result);
            }

            // Converting to Unsigned Int 16 bits reverses the endianess
            result = BitConverter.ToUInt16(response, 5);

            return Response.Create(ResponseStatus.Success, result);
        }

        public class HighSpeedPermissionResult
        {
            public bool IsValid { get; set; }
            public byte DeviceId { get; set; }
            public bool PermissionGranted { get; set; }
        }

        /// <summary>
        /// Parse the response to a request for permission to switch to 4X mode.
        /// </summary>
        public HighSpeedPermissionResult ParseHighSpeedPermissionResponse(Message message)
        {
            byte[] actual = message.GetBytes();
            byte[] granted = new byte[] { Priority.Physical0, DeviceId.Tool, DeviceId.Pcm, Mode.HighSpeedPrepare + Mode.Response };

            // Priority
            if (actual[0] != granted[0])
            {
                return new HighSpeedPermissionResult() { IsValid = false };
            }

            // Destination
            if (actual[1] != granted[1])
            {
                return new HighSpeedPermissionResult() { IsValid = false };
            }

            // Source
            byte moduleId = actual[2];

            // Permission granted?
            if (actual[3] == Mode.HighSpeedPrepare + Mode.Response)
            {
                return new HighSpeedPermissionResult() { IsValid = true, DeviceId = moduleId, PermissionGranted = true };
            }

            if ((actual[3] == Mode.Rejected) || (actual[3] == 0x7F))
            {
                return new HighSpeedPermissionResult() { IsValid = true, DeviceId = moduleId, PermissionGranted = false };
            }

            return new HighSpeedPermissionResult() { IsValid = false };
        }

        public Response<bool> ParseHighSpeedRefusal(Message message)
        {
            byte[] actual = message.GetBytes();
            byte[] refusal = new byte[] { Priority.Physical0, DeviceId.Tool, DeviceId.Broadcast, Mode.HighSpeedPrepare + Mode.Response };

            // Priority
            if (actual[0] != refusal[0])
            {
                return Response.Create(ResponseStatus.UnexpectedResponse, false);
            }

            // Destination
            if (actual[1] != refusal[1])
            {
                return Response.Create(ResponseStatus.UnexpectedResponse, false);
            }

            // Source
            byte moduleId = refusal[2];

            if ((actual[3] == Mode.Rejected) || (actual[3] == 0x7F))
            {
                if (actual[4] == Mode.HighSpeed)
                {
                    return Response.Create(ResponseStatus.Success, true);
                }
            }

            return Response.Create(ResponseStatus.UnexpectedResponse, false);
        }

        /// <summary>
        /// Parse the response to an upload-to-RAM request.
        /// </summary>
        public Response<bool> ParseUploadResponse(Message message)
        {
            return this.DoSimpleValidation(message, 0x6D, 0x36);
        }
                
        /// <summary>
        /// Parse the response to a read request.
        /// </summary>
        public Response<bool> ParseReadResponse(Message message)
        {
            return this.DoSimpleValidation(message, 0x6C, 0x35);

            // collect the 
        }

        /// <summary>
        /// Parse the payload of a read request.
        /// </summary>
        /// <remarks>
        /// It is the callers responsability to check the ResponseStatus for errors
        /// </remarks>
        public Response<byte[]> ParsePayload(Message message, int length, int address)
        {
            ResponseStatus status;
            byte[] actual = message.GetBytes();
            byte[] expected = new byte[] { 0x6D, 0xF0, 0x10, 0x36 };
            if (!TryVerifyInitialBytes(actual, expected, out status))
            {
                return Response.Create(status, new byte[0]);
            }

            if (actual.Length < 10) // 7 byte header, 2 byte sum
            {
                return Response.Create(ResponseStatus.Truncated, new byte[0]);
            }

            int raddr = ((actual[7] << 16) + (actual[8] << 8) + actual[9]);
            if (raddr != address)
            {
                return Response.Create(ResponseStatus.UnexpectedResponse, new byte[0]);
            }

            byte[] result = new byte[length];

            // Normal read
            if (actual[4] == 1)
            {
                //Regular encoding should be an exact match for size
                int rlen = (actual[5] << 8) + actual[6];
                if (rlen != length) // did we get the expected length?
                {
                    return Response.Create(ResponseStatus.Truncated, new byte[0]);
                }

                // Verify block checksum
                UInt16 ValidSum = CalcBlockChecksum(actual);
                int PayloadSum = (actual[rlen + 10] << 8) + actual[rlen + 11];
                Buffer.BlockCopy(actual, 10, result, 0, length);
                if (PayloadSum != ValidSum) return Response.Create(ResponseStatus.Error, result);
            }
            // RLE block
            else if (actual[4] == 2) // TODO check length
            {
                // This isnt going to work with existing kernels... need to support variable length.
                int runLength = actual[5] << 8 + actual[6];
                byte value = actual[10];
                for (int index = 0; index < runLength; index++)
                {
                    result[index] = value;
                }
                return Response.Create(ResponseStatus.Error, result);
            }

            return Response.Create(ResponseStatus.Success, result);
        }
        //TODO: use the copy of this function in VPW.cs
        public UInt16 CalcBlockChecksum(byte[] Block)
        {
            UInt16 Sum = 0;
            int PayloadLength = (Block[5] << 8) + Block[6];

            for (int i = 4; i < PayloadLength + 10; i++) // start after prio, dest, src, mode, stop at end of payload
            {
                Sum += Block[i];
            }

            return Sum;
        }

        /// <summary>
        /// Parse the response to a request for permission to upload a RAM kernel (or part of a kernel).
        /// </summary>
        public Response<bool> ParseUploadPermissionResponse(Message message)
        {
            return this.DoSimpleValidation(message, 0x6C, 0x34);
        }

        public Response<bool> ParseRecoveryModeBroadcast(Message message)
        {
            return this.DoSimpleValidation(message, 0x6C, 0x62, 0x01);
        }

        public Response<bool> ParseStartFullFlashResponse(Message message)
        {
            return this.DoSimpleValidation(message, 0x6C, 0x3C);
        }

        public Response<bool> ParseChunkWriteResponse(Message message)
        {
            return this.DoSimpleValidation(message, 0x6C, 0x36, 0x00, 0x73);
        }

        public Response<bool> ParseFlashLockResponse(Message message)
        {
            return this.DoSimpleValidation(message, 0x6C, 0x36, 0xE0, 0x80);
        }

        public Response<bool> ParseWriteKernelResetResponse(Message message)
        {
            return this.DoSimpleValidation(message, 0x6C, 0x36, 0xE0, 0xAA);
        }

        public Response<bool> ParseFlashKernelSuccessResponse(Message message)
        {
            return this.DoSimpleValidation(message, 0x6C, 0x36, 0xE0, 0x60);
        }


        internal Response<byte> ParseByte(Message responseMessage, byte mode, byte submode)
        {
            ResponseStatus status;
            byte[] expected = { 0x6C, DeviceId.Tool, DeviceId.Pcm, (byte)(mode | 0x40), submode };
            if (!TryVerifyInitialBytes(responseMessage, expected, out status))
            {
                byte[] refused = { 0x6C, DeviceId.Tool, DeviceId.Pcm, 0x7F, mode, submode };
                if (TryVerifyInitialBytes(responseMessage, refused, out status))
                {
                    return Response.Create(ResponseStatus.Refused, (byte)0);
                }

                return Response.Create(status, (byte)0);
            }

            byte[] responseBytes = responseMessage.GetBytes();
            if (responseBytes.Length < 6)
            {
                return Response.Create(ResponseStatus.Truncated, (byte)0);
            }

            int value = responseBytes[5];

            return Response.Create(ResponseStatus.Success, (byte)value);
        }

        internal Response<UInt32> ParseUInt32(Message responseMessage, byte mode, byte submode)
        {
            ResponseStatus status;
            byte[] expected = { 0x6C, DeviceId.Tool, DeviceId.Pcm, (byte)(mode | 0x40), submode };
            if (!TryVerifyInitialBytes(responseMessage, expected, out status))
            {
                byte[] refused = { 0x6C, DeviceId.Tool, DeviceId.Pcm, 0x7F, mode, submode };
                if (TryVerifyInitialBytes(responseMessage, refused, out status))
                {
                    return Response.Create(ResponseStatus.Refused, (UInt32)0);
                }

                return Response.Create(status, (UInt32)0);
            }

            byte[] responseBytes = responseMessage.GetBytes();

            if (responseBytes.Length < 9)
            {
                return Response.Create(ResponseStatus.Truncated, (UInt32)0);
            }

            int value =
                (responseBytes[5] << 24) |
                (responseBytes[6] << 16) |
                (responseBytes[7] << 8) |
                responseBytes[8];

            return Response.Create(ResponseStatus.Success, (UInt32)value);
        }

        internal Response<UInt32> ParseKernelVersion(Message responseMessage)
        {
            return ParseUInt32(responseMessage, 0x3D, 0x00);
        }

        internal Response<UInt32> ParseFlashMemoryType(Message responseMessage)
        {
            return ParseUInt32(responseMessage, 0x3D, 0x01);
        }

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

        internal Response<byte> ParseFlashUnlock(Message message)
        {
            return ParseByte(message, 0x3D, 0x03);
        }

        internal Response<byte> ParseFlashLock(Message message)
        {
            return ParseByte(message, 0x3D, 0x04);
        }

        internal Response<byte> ParseFlashErase(Message message)
        {
            return ParseByte(message, 0x3D, 0x05);
        }


        /// <summary>
        /// Check for an accept/reject message with the given mode byte.
        /// </summary>
        /// <remarks>
        /// TODO: Make this private, use public methods that are tied to a specific message type.
        /// </remarks>
        public Response<bool> DoSimpleValidation(Message message, byte priority, byte mode, params byte[] data)
        {
            byte[] actual = message.GetBytes();
            ResponseStatus status;

            byte[] success = new byte[] { priority, DeviceId.Tool, DeviceId.Pcm, (byte)(mode + 0x40), };
            if (this.TryVerifyInitialBytes(actual, success, out status))
            {
                if (data != null && data.Length > 0)
                {
                    for(int index = 0; index < data.Length; index++)
                    {
                        const int headBytes = 4;
                        int actualLength = actual.Length;
                        int expectedLength = data.Length + headBytes;
                        if (actualLength >= expectedLength)
                        {
                            if (actual[headBytes+index] == data[index])
                            {
                                continue;
                            }
                            else
                            {
                                return Response.Create(ResponseStatus.UnexpectedResponse, false);
                            }
                        }
                        else
                        {
                            return Response.Create(ResponseStatus.Truncated, false);
                        }
                    }
                }

                return Response.Create(ResponseStatus.Success, true);
            }

            byte[] failure = new byte[] { priority, DeviceId.Tool, DeviceId.Pcm, 0x7F, mode };
            if (this.TryVerifyInitialBytes(actual, failure, out status))
            {
                return Response.Create(ResponseStatus.Refused, false);
            }

            return Response.Create(ResponseStatus.UnexpectedResponse, false);
        }

        /// <summary>
        /// Confirm that the first portion of the 'actual' array of bytes matches the 'expected' array of bytes.
        /// </summary>
        private bool TryVerifyInitialBytes(Message actual, byte[] expected, out ResponseStatus status)
        {
            return TryVerifyInitialBytes(actual.GetBytes(), expected, out status);
        }

        /// <summary>
        /// Confirm that the first portion of the 'actual' array of bytes matches the 'expected' array of bytes.
        /// </summary>
        private bool TryVerifyInitialBytes(byte[] actual, byte[] expected, out ResponseStatus status)
        {
            if (actual.Length < expected.Length)
            {
                // This is how we indicate that the response is too short.
                status = ResponseStatus.Truncated;
                return false;
            }

            for (int index = 0; index < expected.Length; index++)
            {
                if (actual[index] != expected[index])
                {
                    // This is how we indicate that the response contained garbage.
                    status = ResponseStatus.UnexpectedResponse;
                    return false;
                }
            }

            status = ResponseStatus.Success;
            return true;
        }

        /// <summary>
        /// Determine whether we were able to unlock the PCM.
        /// </summary>
        public Response<bool> ParseUnlockResponse(byte[] unlockResponse, out string errorMessage)
        {
            if (unlockResponse.Length != 6)
            {
                errorMessage = $"Unlock response was {unlockResponse.Length} bytes long, expected 6.";
                return Response.Create(ResponseStatus.UnexpectedResponse, false);
            }

            byte unlockCode = unlockResponse[5];

            if (unlockCode == 0x34)
            {
                errorMessage = null;
                return Response.Create(ResponseStatus.Success, true);
            }

            switch (unlockCode)
            {
                case 0x36:
                    errorMessage = $"The PCM didn't accept the unlock key value";
                    return Response.Create(ResponseStatus.Error, false);

                case 0x37:
                    errorMessage = $"This PCM is enforcing timeout lock";
                    return Response.Create(ResponseStatus.Timeout, false);

                default:
                    errorMessage = $"Unknown unlock code 0x{unlockCode}";
                    return Response.Create(ResponseStatus.UnexpectedResponse, false);
            }
        }
    }
}
