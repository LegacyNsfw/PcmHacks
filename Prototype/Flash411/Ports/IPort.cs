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
    /// <remarks>
    /// There is no 'Close' method because we use the Dispose method from IDisposable instead.
    /// </remarks>
    interface IPort : IDisposable
    {
        Task OpenAsync(PortConfiguration configuration);

        Task Send(byte[] buffer);

        Task<int> Receive(byte[] buffer, int offset, int count);
    }

    class PortConfiguration
    {
    }

    class SerialPortConfiguration : PortConfiguration
    {
        public int BaudRate { get; set; }
    }
}
