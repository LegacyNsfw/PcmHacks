using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
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
    class MessageParser
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
        public Response<UInt32> ParseOperatingSystemId(byte[] response)
        {
            int result = 0;
            ResponseStatus status;

            byte[] expected = new byte[] { 0x6C, 0xF0, 0x10, 0x7C, BlockId.OperatingSystemID };
            if (!TryVerifyInitialBytes(response, expected, out status))
            {
                return Response.Create(ResponseStatus.Error, (UInt32)result);
            }

            result = response[5] << 24;
            result += response[6] << 16;
            result += response[7] << 8;
            result += response[8];

            return Response.Create(ResponseStatus.Success, (UInt32)result);
        }

        /// <summary>
        /// Parse the responses to the three requests for VIN information.
        /// </summary>
        public Response<string> ParseVinResponses(byte[] response1, byte[] response2, byte[] response3)
        {
            string result = "Unknown";
            ResponseStatus status;

            byte[] expected = new byte[] { 0x6C, 0xF0, 0x10, 0x7C, BlockId.Vin1 };
            if (!TryVerifyInitialBytes(response1, expected, out status))
            {
                return Response.Create(status, result);
            }

            expected = new byte[] { 0x6C, 0xF0, 0x10, 0x7C, BlockId.Vin2 };
            if (!TryVerifyInitialBytes(response2, expected, out status))
            {
                return Response.Create(status, result);
            }

            expected = new byte[] { 0x6C, 0xF0, 0x10, 0x7C, BlockId.Vin3 };
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

            byte[] expected = new byte[] { 0x6C, 0xF0, 0x10, 0x7C, BlockId.Serial1 };
            if (!TryVerifyInitialBytes(response1, expected, out status))
            {
                return Response.Create(status, result);
            }

            expected = new byte[] { 0x6C, 0xF0, 0x10, 0x7C, BlockId.Serial2 };
            if (!TryVerifyInitialBytes(response2, expected, out status))
            {
                return Response.Create(status, result);
            }

            expected = new byte[] { 0x6C, 0xF0, 0x10, 0x7C, BlockId.Serial3 };
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

        public Response<string> ParseBCCresponse(byte[] response)
        {
            string result = "Unknown";
            ResponseStatus status;

            byte[] expected = new byte[] { 0x6C, 0xF0, 0x10, 0x7C, BlockId.BCC };
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

        public Response<string> ParseMECresponse(byte[] response)
        {
            string result = "Unknown";
            ResponseStatus status;

            byte[] expected = new byte[] { 0x6C, 0xF0, 0x10, 0x7C, BlockId.MEC };
            if (!TryVerifyInitialBytes(response, expected, out status))
            {
                return Response.Create(status, result);
            }

            string MEC = response[5].ToString();
            
            return Response.Create(ResponseStatus.Success, MEC);
        }

        /// <summary>
        /// Parse the response to a seed request.
        /// </summary>
        public Response<UInt16> ParseSeed(byte[] response)
        {
            ResponseStatus status;
            UInt16 result = 0;

            byte[] unlocked = { 0x6C, 0x70, 0x10, 0x67, 0x01, 0x37 };
            byte[] seed = new byte[] { 0x6C, 0xF0, 0x10, 0x67, 0x01, };

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
        internal Response<bool> ParseUnlockResponse(byte[] unlockResponse, out string errorMessage)
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
