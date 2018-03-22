using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// This class is responsible for sending and receiving data over a J2534 passthrough device.
    /// </summary>
    class J2534Port : IPort
    {
        private const string PortName = "J2534";

        /// <summary>
        /// This returns the string that appears in the drop-down list.
        /// </summary>
        public override string ToString()
        {
            return PortName;
        }

        /// <summary>
        /// Open the J2534 device.
        /// </summary>
        Task IPort.OpenAsync(PortConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Close the J2534 device.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Send a sequence of bytes.
        /// </summary>
        Task IPort.Send(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Receive a sequence of bytes.
        /// </summary>
        Task<int> IPort.Receive(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Discard anything in the input and output buffers.
        /// </summary>
        public Task DiscardBuffers()
        {
            return Task.FromResult(0);
        }

        public static bool IsPresent()
        {
            // TODO: Add code to determine whether a J2534 device is present.
            return false;
        }

        Task<int> IPort.GetReceiveQueueSize()
        {
            return Task.FromResult(0);
        }
    }
}
