using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// This class encapsulates all code that is unique to the Arduino-based interface that Thaniel has created.
    /// </summary>
    class ThanielDevice : SerialDevice
    {
        public const string DeviceType = "Thaniel";

        public ThanielDevice(IPort port, ILogger logger) : base (port, logger)
        {

        }

        public override string GetDeviceType()
        {
            return DeviceType;
        }

        public override Task<bool> Initialize()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Send a message, do not expect a response.
        /// </summary>
        public override Task<bool> SendMessage(Message message)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Send a message, wait for a response, return the response.
        /// </summary>
        public override Task<Response<Message>> SendRequest(Message message)
        {
            return Task.FromResult(Response.Create(ResponseStatus.Success, new Message(new byte[] { })));
        }

        /// <summary>
        /// Set the interface to low (false) or high (true) speed
        /// </summary>
        /// <remarks>
        /// The caller must also tell the PCM to switch speeds
        /// </remarks>
        public override Task<bool> SetVPW4x(bool highspeed)
        {
            if (!highspeed)
            {
                this.Logger.AddDebugMessage("Not Implemented: Thaniel setting VPW 1X");
            }
            else
            {
                this.Logger.AddDebugMessage("Not Implemented: Thaniel setting VPW 4X");
            }

            return Task.FromResult(false);
        }
    }
}
