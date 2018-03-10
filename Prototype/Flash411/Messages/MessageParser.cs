using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    class MessageParser
    {
        public MessageParser()
        {
        }

        public Response<UInt32> ParseOperatingSystemId(Response<byte[]> response)
        {
            UInt32 result = 0;
            ResponseStatus status;

            byte[] expected = new byte[] { 0x6C, 0xF0, 0x10, 0x7C, BlockId.OperatingSystemId };
            if (!TryVerifyInitialBytes(response.Value, expected, out status))
            {
                return Response.Create(status, result);
            }

            result = BitConverter.ToUInt32(response.Value, 5);
            return Response.Create(response.Status, result);
        }

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
            Buffer.BlockCopy(response1.Value, 5, vinBytes, 0, 5);
            Buffer.BlockCopy(response2.Value, 5, vinBytes, 5, 4);
            Buffer.BlockCopy(response3.Value, 5, vinBytes, 9, 4);
            string vin = System.Text.Encoding.ASCII.GetString(vinBytes);
            return Response.Create(ResponseStatus.Success, vin);
        }

        public Response<UInt16> ParseSeed(Response<byte[]> response)
        {
            UInt16 result = 0;
            ResponseStatus status;

            byte[] expected = new byte[] { 0x6C, 0x10, 0xF0, 0x27, 0x02, };
            if (!TryVerifyInitialBytes(response.Value, expected, out status))
            {
                return Response.Create(status, result);
            }

            result = BitConverter.ToUInt16(response.Value, 5);
            return Response.Create(ResponseStatus.Success, result);
        }

        private bool TryVerifyInitialBytes(byte[] actual, byte[] expected, out ResponseStatus status)
        {
            if (actual.Length < expected.Length)
            {
                status = ResponseStatus.Truncated;
                return false;
            }

            for (int index = 0; index < expected.Length; index++)
            {
                if (actual[index] != expected[index])
                {
                    status = ResponseStatus.UnexpectedResponse;
                    return false;
                }
            }

            status = ResponseStatus.Success;
            return true;
        }
    }
}
