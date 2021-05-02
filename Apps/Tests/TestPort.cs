using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// This class allows test cases to specify data to receive, and examine data that was sent.
    /// </summary>
    public class TestPort : IPort
    {
        public List<byte[]> MessagesSent { get; }

        public MemoryStream BytesToReceive { get; }

        public TestPort(ILogger logger)
        {
            this.MessagesSent = new List<byte[]>();
            this.BytesToReceive = new MemoryStream();
        }

        public void EnqueueBytes(byte[] bytes)
        {
            this.BytesToReceive.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// This returns the string that appears in the drop-down list.
        /// </summary>
        public override string ToString()
        {
            return "Test Port";
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
            this.MessagesSent.Add(buffer);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Receive bytes from the mock PCM.
        /// </summary>
        Task<int> IPort.Receive(byte[] buffer, int offset, int count)
        {
            BytesToReceive.Read(buffer, offset, count);
            return Task.FromResult(count);
        }

        /// <summary>
        /// Discard anything in the input and output buffers.
        /// </summary>
        public Task DiscardBuffers()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Sets the read timeout.
        /// </summary>
        public void SetTimeout(int milliseconds)
        {
        }

        /// <summary>
        /// Indicates the number of bytes waiting in the queue.
        /// </summary>
        Task<int> IPort.GetReceiveQueueSize()
        {
            return Task.FromResult((int)(this.BytesToReceive.Length - this.BytesToReceive.Position));
        }
    }
}
