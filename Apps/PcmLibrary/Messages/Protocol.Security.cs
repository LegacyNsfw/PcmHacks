using System;
using System.Collections.Generic;
using System.Text;

namespace PcmHacking
{
    public partial class Protocol
    {
        /// <summary>
        /// Create a request to retrieve a 'seed' value from the PCM
        /// </summary>
        public Message CreateSeedRequest()
        {
            byte[] Bytes = new byte[] { Priority.Physical0, DeviceId.Pcm, DeviceId.Tool, Mode.Seed, SubMode.GetSeed };
            return new Message(Bytes);
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

        /// <summary>
        /// Create a request to send a 'key' value to the PCM
        /// </summary>
        public Message CreateUnlockRequest(UInt16 Key)
        {
            byte KeyHigh = (byte)((Key & 0xFF00) >> 8);
            byte KeyLow = (byte)(Key & 0xFF);
            byte[] Bytes = new byte[] { Priority.Physical0, DeviceId.Pcm, DeviceId.Tool, Mode.Seed, SubMode.SendKey, KeyHigh, KeyLow };
            return new Message(Bytes);
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
    }
}
