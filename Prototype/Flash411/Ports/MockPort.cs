using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// This class is just here to enable testing without any actual interface hardware.
    /// </summary>
    /// <remarks>
    /// Eventually the Receive method should return simulated VPW responses.
    /// </remarks>
    class MockPort : IPort
    {
        public const string PortName = "Mock Port";

        /// <summary>
        /// This returns the string that appears in the drop-down list.
        /// </summary>
        public override string ToString()
        {
            return PortName;
        }

        /// <summary>
        /// Pretend to open a port.
        /// </summary>
        Task IPort.Open()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Pretend to close a port.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Pretend to send a sequence of bytes.
        /// </summary>
        Task IPort.Send(byte[] buffer)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Pretend to receive a sequence of bytes.
        /// </summary>
        Task<int> IPort.Receive(byte[] buffer, int offset, int count)
        {
            return Task.FromResult(buffer.Length);
        }
    }
}
