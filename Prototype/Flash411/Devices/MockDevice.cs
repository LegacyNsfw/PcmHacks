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
        public MockDevice(IPort port) : base(port)
        {

        }

        public override string ToString()
        {
            return "Mock Interface";
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
            return Task.FromResult(true);
        }

        /// <summary>
        /// Send a message, wait for a response, return the response.
        /// </summary>
        public override Task<Response<byte[]>> SendRequest(Message message)
        {
            return Task.FromResult(Response.Create(ResponseStatus.Success, new byte[] { }));
        }
    }
}
