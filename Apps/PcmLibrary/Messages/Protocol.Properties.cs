using System;
using System.Collections.Generic;
using System.Text;

namespace PcmHacking
{
    public partial class Protocol
    {
        /// <summary>
        /// Create a request to read the given block of PCM memory.
        /// </summary>
        public Message CreateReadRequest(byte Block)
        {
            byte[] Bytes = new byte[] { Priority.Physical0, DeviceId.Pcm, DeviceId.Tool, Mode.ReadBlock, Block };
            return new Message(Bytes);
        }

        /// <summary>
        /// Create a request to read the PCM's operating system ID.
        /// </summary>
        /// <returns></returns>
        public Message CreateOperatingSystemIdReadRequest()
        {
            return CreateReadRequest(BlockId.OperatingSystemID);
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

            byte[] expected = new byte[] { Priority.Physical0, DeviceId.Tool, DeviceId.Pcm, responseMode };
            if (!TryVerifyInitialBytes(bytes, expected, out status))
            {
                return Response.Create(ResponseStatus.Error, (UInt32)result);
            }
            if (bytes.Length < 9)
            {
                return Response.Create(ResponseStatus.Truncated, (UInt32)result);
            }

            result = bytes[5] << 24;
            result += bytes[6] << 16;
            result += bytes[7] << 8;
            result += bytes[8];

            return Response.Create(ResponseStatus.Success, (UInt32)result);
        }

        /// <summary>
        /// Parse the response to a block-read request.
        /// </summary>
        public Response<UInt32> ParseUInt32FromBlockReadResponse(Message message)
        {
            return ParseUInt32(message, Mode.ReadBlock + Mode.Response);
        }

        #region VIN

        /// <summary>
        /// Create a request to read the first segment of the PCM's VIN.
        /// </summary>
        public Message CreateVinRequest1()
        {
            return CreateReadRequest(BlockId.Vin1);
        }

        /// <summary>
        /// Create a request to read the second segment of the PCM's VIN.
        /// </summary>
        public Message CreateVinRequest2()
        {
            return CreateReadRequest(BlockId.Vin2);
        }

        /// <summary>
        /// Create a request to read the thid segment of the PCM's VIN.
        /// </summary>
        public Message CreateVinRequest3()
        {
            return CreateReadRequest(BlockId.Vin3);
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

        #endregion

        #region Serial

        /// <summary>
        /// Create a request to read the first segment of the PCM's Serial Number.
        /// </summary>
        public Message CreateSerialRequest1()
        {
            return CreateReadRequest(BlockId.Serial1);
        }

        /// <summary>
        /// Create a request to read the second segment of the PCM's Serial Number.
        /// </summary>
        public Message CreateSerialRequest2()
        {
            return CreateReadRequest(BlockId.Serial2);
        }

        /// <summary>
        /// Create a request to read the thid segment of the PCM's Serial Number.
        /// </summary>
        public Message CreateSerialRequest3()
        {
            return CreateReadRequest(BlockId.Serial3);
        }

        /// <summary>
        /// Parse the responses to the three requests for Serial Number information.
        /// </summary>
        public Response<string> ParseSerialResponses(Message response1, Message response2, Message response3)
        {
            string result = "Unknown";
            ResponseStatus status;

            byte[] expected = new byte[] { Priority.Physical0, DeviceId.Tool, DeviceId.Pcm, Mode.ReadBlock + Mode.Response, BlockId.Serial1 };
            if (!TryVerifyInitialBytes(response1, expected, out status))
            {
                return Response.Create(status, result);
            }

            expected = new byte[] { Priority.Physical0, DeviceId.Tool, DeviceId.Pcm, Mode.ReadBlock + Mode.Response, BlockId.Serial2 };
            if (!TryVerifyInitialBytes(response2, expected, out status))
            {
                return Response.Create(status, result);
            }

            expected = new byte[] { Priority.Physical0, DeviceId.Tool, DeviceId.Pcm, Mode.ReadBlock + Mode.Response, BlockId.Serial3 };
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

        #endregion

        #region BCC

        /// <summary>
        /// Create a request to read the Broad Cast Code (BCC).
        /// </summary>
        public Message CreateBCCRequest()
        {
            return CreateReadRequest(BlockId.BCC);
        }

        public Response<string> ParseBCCresponse(Message responseMessage)
        {
            string result = "Unknown";
            ResponseStatus status;
            byte[] response = responseMessage.GetBytes();

            byte[] expected = new byte[] { Priority.Physical0, DeviceId.Tool, DeviceId.Pcm, Mode.ReadBlock + Mode.Response, BlockId.BCC };
            if (!TryVerifyInitialBytes(response, expected, out status))
            {
                return Response.Create(status, result);
            }

            byte[] BCCBytes = new byte[4];
            Buffer.BlockCopy(response, 5, BCCBytes, 0, 4);

            byte[] printableBytes = Utility.GetPrintable(BCCBytes);
            string BCC = System.Text.Encoding.ASCII.GetString(printableBytes);

            return Response.Create(ResponseStatus.Success, BCC);
        }

        #endregion

        #region MEC

        /// <summary>
        /// Create a request to read the Broad Cast Code (MEC).
        /// </summary>
        public Message CreateMECRequest()
        {
            return CreateReadRequest(BlockId.MEC);
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

        #endregion
    }
}
