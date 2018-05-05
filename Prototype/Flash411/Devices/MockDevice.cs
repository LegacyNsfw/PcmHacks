using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// This class provides a way to test most of the app without any interface hardware.
    /// </summary>
    public class MockDevice : SerialDevice
    {
        public const string DeviceType = "Mock Serial Device";

        public MockDevice(IPort port, ILogger logger) : base(port, logger)
        {

        }

        public override string GetDeviceType()
        {
            return DeviceType;
        }

        public override Task<bool> Initialize()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Send a message, do not expect a response.
        /// </summary>
        public override Task<bool> SendMessage(Message message)
        {
            StringBuilder builder = new StringBuilder();
            this.Logger.AddDebugMessage("Sending message " + message.GetBytes().ToHex());
            this.Port.Send(message.GetBytes());
            return Task.FromResult(true);
        }

        /// <summary>
        /// Send a message, wait for a response, return the response.
        /// </summary>
        public async override Task<Response<Message>> SendRequest(Message message)
        {
            StringBuilder builder = new StringBuilder();
            this.Logger.AddDebugMessage("Sending request " + message.GetBytes().ToHex());
            await this.Port.Send(message.GetBytes());

            byte[] responseBuffer = new byte[100];
            int received = await this.Port.Receive(responseBuffer, 0, 100);
            List<byte> actualResponse = new List<byte>(responseBuffer.Take(received));

            return Response.Create(ResponseStatus.Success, new Message(actualResponse.ToArray()));
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
                this.Logger.AddDebugMessage("Setting VPW 1X");
            }
            else
            {
                this.Logger.AddDebugMessage("Setting VPW 4X");
            }

            return Task.FromResult(true);
        }
    }
}
