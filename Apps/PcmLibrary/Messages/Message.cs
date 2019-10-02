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
        /// When the message was created.
        /// </summary>
        private ulong timestamp;

        /// <summary>
        /// Error code, if applicable.
        /// </summary>
        private ulong error;

        /// <summary>
        /// Returns the length of the message.
        /// </summary>
        public int Length
        {
            get
            {
                return this.message.Length;
            }
        }

        /// <summary>
        /// Get the Nth byte of the message.
        /// </summary>
        public byte this[int index]
        {
            get
            {
                return this.message[index];
            }
        }

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
        public Message(byte[] message, ulong timestamp, ulong error)
        {
            this.message = message;
            this.timestamp = timestamp;
            this.error = error;
        }

        /// <summary>
        /// When the message was created or recevied.
        /// </summary>
        public ulong TimeStamp
        {
            get { return this.timestamp; }
            set { this.timestamp = value; }
        }

        /// <summary>
        /// The error associated with creating or receiving this message.
        /// </summary>
        public ulong Error
        {
            get { return this.error; }
            set { this.error = value; }
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
        public override string ToString()
        {
            return string.Join(" ", Array.ConvertAll(message, b => b.ToString("X2")));
        }
    }
}
