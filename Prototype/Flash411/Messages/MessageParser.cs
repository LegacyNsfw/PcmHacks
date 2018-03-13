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
        public Response<UInt32> ParseOperatingSystemId(Response<byte[]> response)
        {
            int result = 0;
            ResponseStatus status;

            byte[] expected = new byte[] { 0x6C, 0xF0, 0x10, 0x7C, BlockId.OperatingSystemID };
            if (!TryVerifyInitialBytes(response.Value, expected, out status))
            {
                return Response.Create(status, (UInt32)result);
            }

            result = response.Value[5] << 24;
            result += response.Value[6] << 16;
            result += response.Value[7] << 8;
            result += response.Value[8];

            return Response.Create(response.Status, (UInt32)result);
        }

        /// <summary>
        /// Parse the responses to the three requests for VIN information.
        /// </summary>
        public Response<string> ParseVinResponses(Response<byte[]> response1, Response<byte[]> response2, Response<byte[]> response3)
        {
            string result = "Unknown";
            ResponseStatus status;

            byte[] expected = new byte[] { 0x6C, 0xF0, 0x10, 0x7C, BlockId.Vin1 };
            if (!TryVerifyInitialBytes(response1.Value, expected, out status))
            {
                return Response.Create(status, result);
            }

            expected = new byte[] { 0x6C, 0xF0, 0x10, 0x7C, BlockId.Vin2 };
            if (!TryVerifyInitialBytes(response2.Value, expected, out status))
            {
                return Response.Create(status, result);
            }

            expected = new byte[] { 0x6C, 0xF0, 0x10, 0x7C, BlockId.Vin3 };
            if (!TryVerifyInitialBytes(response3.Value, expected, out status))
            {
                return Response.Create(status, result);
            }

            byte[] vinBytes = new byte[13];
            Buffer.BlockCopy(response1.Value, 6, vinBytes, 0, 5);
            Buffer.BlockCopy(response2.Value, 6, vinBytes, 5, 4);
            Buffer.BlockCopy(response3.Value, 6, vinBytes, 9, 4);
            string vin = System.Text.Encoding.ASCII.GetString(vinBytes);
            return Response.Create(ResponseStatus.Success, vin);
        }

        /// <summary>
        /// Parse the response to a seed request.
        /// </summary>
        public Response<UInt16> ParseSeed(Response<byte[]> response)
        {
            UInt16 result = 0;
            ResponseStatus status;

            byte[] expected = new byte[] { 0x6C, 0xF0, 0x10, 0x67, 0x01, };
            if (!TryVerifyInitialBytes(response.Value, expected, out status))
            {
                return Response.Create(status, result);
            }

            // Do we need to byte-swap this value?
            result = BitConverter.ToUInt16(response.Value, 5);

            return Response.Create(ResponseStatus.Success, result);
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
    }
}
