using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// Message is a thin wrapper around an array of bytes.
    /// </summary>
    /// <remarks>
    /// I'll admit that this might be overkill as opposed to just passing around byte arrays.
    /// But the ToString method makes messages easier to view in the debugger.
    /// </remarks>
    public class Message
    {
        /// <summary>
        /// The message content.
        /// </summary>
        private byte[] message;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Message(byte[] message)
        {
            this.message = message;
        }
         
        /// <summary>
        /// Constructor.
        /// </summary>
        public Message(byte[] message, ulong Timestamp, ulong Error)
        {
            this.message = message;
            this._Timestamp = Timestamp;
            this._Error = Error;
        }

        /// <summary>
        /// When the message was created or recevied.
        /// </summary>
        private ulong _Timestamp;
        public ulong TimeStamp
        {
            get { return this._Timestamp; }
            set { this._Timestamp = value; }
        }

        /// <summary>
        /// The error associated with creating or receiving this message.
        /// </summary>
        private ulong _Error;
        public ulong Error
        {
            get { return this.Error; }
            set { this.Error = value; }
        }

        /// <summary>
        /// Get the raw bytes.
        /// </summary>
        /// <returns></returns>
        [DebuggerStepThrough]
        public byte[] GetBytes()
        {
            return this.message;
        }

        /// <summary>
        /// Generate a descriptive string for this message.
        /// </summary>
        /// <remarks>
        /// This is the most valuable thing - it makes messages easy to view in the debugger.
        /// </remarks>
        public string GetString()
        {
            return string.Join(" ", Array.ConvertAll(message, b => b.ToString("X2")));
        }
    }
}
