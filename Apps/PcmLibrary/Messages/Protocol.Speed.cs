using System;
using System.Collections.Generic;
using System.Text;

namespace PcmHacking
{
    public partial class Protocol
    {
        public class HighSpeedPermissionResult
        {
            public bool IsValid { get; set; }
            public byte DeviceId { get; set; }
            public bool PermissionGranted { get; set; }
        }

        /// <summary>
        /// Create a request for a module to test VPW speed switch to 4x is OK
        /// </summary>
        public Message CreateHighSpeedPermissionRequest(byte deviceId)
        {
            return new Message(new byte[] { Priority.Physical0, deviceId, DeviceId.Tool, Mode.HighSpeedPrepare });
        }

        /// <summary>
        /// Create a request for a specific module to switch to VPW 4x
        /// </summary>
        public Message CreateBeginHighSpeed(byte deviceId)
        {
            return new Message(new byte[] { Priority.Physical0, deviceId, DeviceId.Tool, Mode.HighSpeed });
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
    }
}
