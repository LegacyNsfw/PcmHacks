using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// This class provides a way to test most of the app without any interface hardware.
    /// </summary>
    public class MockDevice : Device
    {
        /// <summary>
        /// Device ID string to use in the Device Picker form, and in interal device-type comparisons.
        /// </summary>
        public const string DeviceType = "Mock Serial Device";

        /// <summary>
        /// The mock port.
        /// </summary>
        private IPort port;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public MockDevice(IPort port, ILogger logger) : base(logger)
        {
            this.port = port;
        }

        /// <summary>
        /// Not actually necessary for this device type, but since we need to implement IDisposable...
        /// </summary>
        protected override void Dispose(bool disposing)
        {    
        }
        
        /// <summary>
        /// Initialize the device. It's just a no-op for this device type.
        /// </summary>
        public override Task<bool> Initialize()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Not needed.
        /// </summary>
        public override Task<TimeoutScenario> SetTimeout(TimeoutScenario scenario)
        {
            return Task.FromResult(this.currentTimeoutScenario);
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
        protected override Task<bool> SetVpwSpeedInternal(VpwSpeed newSpeed)
        {
            if (newSpeed == VpwSpeed.Standard)
            {
                this.Logger.AddDebugMessage("Setting VPW 1X");
            }
            else
            {
                this.Logger.AddDebugMessage("Setting VPW 4X");
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Purse any messages in the incoming-message buffer.
        /// </summary>
        public override void ClearMessageBuffer()
        {
            this.port.DiscardBuffers();
        }
    }
}
