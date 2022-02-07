using System;
using System.Collections.Generic;
using System.Text;

namespace PcmHacking
{
    public partial class Protocol
    {
        class Security
        {
            public const byte Denied  = 0x33; // Security Access Denied
            public const byte Allowed = 0x34; // Security Access Allowed
            public const byte Invalid = 0x35; // Invalid Key
            public const byte TooMany = 0x36; // Exceed Number of Attempts
            public const byte Delay  = 0x37; // Required Time Delay Not Expired
        }


        /// <summary>
        /// Create a request to retrieve a 'seed' value from the PCM
        /// </summary>
        public Message CreateSeedRequest()
        {
            byte[] Bytes = new byte[] { Priority.Physical0, DeviceId.Pcm, DeviceId.Tool, Mode.Seed, Submode.GetSeed };
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
            byte[] seed = new byte[] { Priority.Physical0, DeviceId.Tool, DeviceId.Pcm, Mode.Seed + Mode.Response, 0x01 };

            if (TryVerifyInitialBytes(response, unlocked, out status))
            {
                status = ResponseStatus.Success;
                return Response.Create(ResponseStatus.Success, result);
            }

            if (!TryVerifyInitialBytes(response, seed, out status))
            {
                return Response.Create(ResponseStatus.Error, result);
            }

            // Let's not reverse endianess
            result = (UInt16)((response[5] << 8) | response[6]);

            return Response.Create(ResponseStatus.Success, result);
        }

        /// <summary>
        /// Create a request to send a 'key' value to the PCM
        /// </summary>
        public Message CreateUnlockRequest(UInt16 Key)
        {
            byte KeyHigh = (byte)((Key & 0xFF00) >> 8);
            byte KeyLow = (byte)(Key & 0xFF);
            byte[] Bytes = new byte[] { Priority.Physical0, DeviceId.Pcm, DeviceId.Tool, Mode.Seed, Submode.SendKey, KeyHigh, KeyLow };
            return new Message(Bytes);
        }

        /// <summary>
        /// Determine whether we were able to unlock the PCM.
        /// </summary>
        public Response<bool> ParseUnlockResponse(byte[] unlockResponse, out string errorMessage)
        {
            if (unlockResponse.Length < 6)
            {
                errorMessage = $"Unlock response truncated, expected 6 bytes, got {unlockResponse.Length} bytes.";
                return Response.Create(ResponseStatus.UnexpectedResponse, false);
            }

            byte unlockCode = unlockResponse[5];

            switch (unlockCode)
            {
                case Security.Allowed:
                    errorMessage = null;
                    return Response.Create(ResponseStatus.Success, true);

                case Security.Denied:
                    errorMessage = $"The PCM refused to unlock";
                    return Response.Create(ResponseStatus.Error, false);

                case Security.Invalid:
                    errorMessage = $"The PCM didn't accept the unlock key value";
                    return Response.Create(ResponseStatus.Error, false);

                case Security.TooMany:
                    errorMessage = $"The PCM did not accept the key - too many attempts";
                    return Response.Create(ResponseStatus.Error, false);

                case Security.Delay:
                    errorMessage = $"The PCM is enforcing timeout lock";
                    return Response.Create(ResponseStatus.Timeout, false);
                    
                default:
                    errorMessage = $"Unknown unlock response code: 0x{unlockCode:X2}";
                    return Response.Create(ResponseStatus.UnexpectedResponse, false);
            }
        }

        /// <summary>
        /// Indicates whether or not the reponse indicates that the PCM is unlocked.
        /// </summary>
        public bool IsUnlocked(byte[] response)
        {
            ResponseStatus status;
            byte[] unlocked = { Priority.Physical0, DeviceId.Tool, DeviceId.Pcm, Mode.Seed + Mode.Response, 0x01, 0x37 };

            if (TryVerifyInitialBytes(response, unlocked, out status))
            {
                // To short to be a seed?
                if (response.Length < 7)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
