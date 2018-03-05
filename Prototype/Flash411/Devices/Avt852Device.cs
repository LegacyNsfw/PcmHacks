using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// This class encapsulates all code that is unique to the AVT 852 interface.
    /// </summary>
    class Avt852Device : Device
    {
        public Avt852Device(IPort port) : base(port)
        {
        }

        public override string ToString()
        {
            return "AVT 852";
        }

        public override Task<bool> Initialize()
        {
            throw new NotImplementedException();
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
