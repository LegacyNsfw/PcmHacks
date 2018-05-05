using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// Derived classes will emulate AVT, ScanTool, etc, by stripping control
    /// data and passing messages through to the underlying port, and by adding
    /// metadata to messages received from the underlying port.
    /// </summary>
    public class DeviceEmulator : IPort
    {
        IPort port;

        public DeviceEmulator(IPort port)
        {
            this.port = port;
        }
        
        /// <summary>
        /// This returns the string that appears in the drop-down list.
        /// </summary>
        public override string ToString()
        {
            return "Device Emulator";
        }

        /// <summary>
        /// Pretend to open a port.
        /// </summary>
        Task IPort.OpenAsync(PortConfiguration configuration)
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
        /// Send bytes to the mock PCM.
        /// </summary>
        Task IPort.Send(byte[] buffer)
        {
            return this.port.Send(buffer);
        }

        /// <summary>
        /// Receive bytes from the mock PCM.
        /// </summary>
        Task<int> IPort.Receive(byte[] buffer, int offset, int count)
        {
            return this.port.Receive(buffer, offset, count);
        }

        /// <summary>
        /// Discard anything in the input and output buffers.
        /// </summary>
        public Task DiscardBuffers()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Indicates the number of bytes waiting in the queue.
        /// </summary>
        Task<int> IPort.GetReceiveQueueSize()
        {
            // return Task.FromResult(0);
            throw new NotImplementedException();
        }
    }
}
