using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// Not sure if we'll really use this class...
    /// </summary>
    class Response
    {
        private byte[] buffer;
        public Response()
        {
            this.buffer = new byte[8 * 1024];
        }

        public byte[] GetBuffer()
        {
            return this.buffer;
        }
    }

    /// <summary>
    /// The IPort implementations encapsulate the differences between serial 
    /// ports, J2534 passthrough devices, and whatever else we end up using.
    /// </summary>
    /// <remarks>
    /// There is no 'Close' method because we use the Dispose method from IDisposable instead.
    /// </remarks>
    interface IPort : IDisposable
    {
        Task Open();

        Task Send(byte[] buffer);

        Task<int> Receive(byte[] buffer, int offset, int count);
    }
}
