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

        public Message(byte[] message, ulong Timestamp, ulong Error)
        {
            this.message = message;
            this._Timestamp = Timestamp;
            this._Error = Error;
        }

        private ulong _Timestamp;
        public ulong TimeStamp
        {
            get { return this._Timestamp; }
            set { this._Timestamp = value; }
        }
        private ulong _Error;
        public ulong Error
        {
            get { return this.Error; }
            set { this.Error = value; }
        }


        [DebuggerStepThrough]
        public byte[] GetBytes()
        {
            return this.message;
        }

        public string GetString()
        {
            return string.Join(" ", Array.ConvertAll(message, b => b.ToString("X2")));
        }
    }
}
