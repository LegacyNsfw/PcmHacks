using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// The IPort implementations encapsulate the differences between serial 
    /// ports, J2534 passthrough devices, and whatever else we end up using.
    /// </summary>
    interface IPort : IDisposable
    {
        /// <summary>
        /// Open the port.
        /// </summary>
        Task OpenAsync(PortConfiguration configuration);

        /// <summary>
        /// Send a sequence of bytes.
        /// </summary>
        Task Send(byte[] buffer);

        /// <summary>
        /// Receive a buffer of bytes.
        /// </summary>
        Task<int> Receive(byte[] buffer, int offset, int count);

        /// <summary>
        /// Discard anything in the input and output buffers.
        /// </summary>
        Task DiscardBuffers();
    }

    class PortConfiguration
    {
    }

    class SerialPortConfiguration : PortConfiguration
    {
        public int BaudRate { get; set; }
    }
}
