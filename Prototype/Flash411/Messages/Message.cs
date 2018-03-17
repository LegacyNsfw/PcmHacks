using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// Message is a thin wrapper around an array of bytes.
    /// </summary>
    class Message
    {
        private byte[] message;

        public Message(byte[] message)
        {
            this.message = message;
        }

        [DebuggerStepThrough]
        public byte[] GetBytes()
        {
            return this.message;
        }

        public string GetString()
        {
            return string.Join(string.Empty, Array.ConvertAll(message, b => b.ToString("X2")));
        }
    }
}
