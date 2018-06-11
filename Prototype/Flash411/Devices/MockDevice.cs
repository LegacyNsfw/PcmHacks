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
    class MockDevice : Device
    {
        public const string DeviceType = "Mock Serial Device";
        private IPort port;
        
        public MockDevice(IPort port, ILogger logger) : base(logger)
        {
            this.port = port;
        }

        protected override void Dispose(bool disposing)
        {    
        }
        
        public override Task<bool> Initialize()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Not needed.
        /// </summary>
        public override Task SetTimeout(TimeoutScenario scenario)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Send a message, do not expect a response.
        /// </summary>
        public override Task<bool> SendMessage(Message message)
        {
            StringBuilder builder = new StringBuilder();
            this.Logger.AddDebugMessage("Sending message " + message.GetBytes().ToHex());
            this.port.Send(message.GetBytes());
            return Task.FromResult(true);
        }

        /// <summary>
        /// Try to read an incoming message from the device.
        /// </summary>
        /// <returns></returns>
        protected async override Task Receive()
        {
            //List<byte> incoming = new List<byte>(5000);
            byte[] incoming = new byte[5000];
            int count = await this.port.Receive(incoming, 0, incoming.Length);
            if(count > 0)
            {
                byte[] sized = new byte[count];
                Buffer.BlockCopy(incoming, 0, sized, 0, count);
                base.Enqueue(new Message(sized));
            }

            return;
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

        public override void ClearMessageBuffer()
        {
            this.port.DiscardBuffers();
        }
    }
}
