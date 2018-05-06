using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Flash411;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    /// <summary>
    /// Implements the IPort interface atop a MockPcm instance.
    /// </summary>
    class MockPcmPort : IPort
    {
        private MockPcm mockPcm;

        public MockPcmPort(MockPcm mockPcm)
        {
            this.mockPcm = mockPcm;
        }
        
        public void Dispose()
        {
        }

        public Task DiscardBuffers()
        {
            return Task.FromResult(0);
        }

        public Task<int> GetReceiveQueueSize()
        {
            return Task.FromResult(0);
        }

        public Task OpenAsync(PortConfiguration configuration)
        {
            return Task.FromResult(0);
        }

        public Task<int> Receive(byte[] buffer, int offset, int count)
        {
            byte[] response = this.mockPcm.GetResponse();
            int bytesCopied = 0;
            for (int index = 0; index < count && index < response.Length; index++)
            {
                buffer[index] = response[index];
                bytesCopied++;
            }

            return Task.FromResult(bytesCopied);
        }

        public Task Send(byte[] buffer)
        {
            for (int index = 0; index < buffer.Length; index++)
            {
                this.mockPcm.Push(buffer[index]);
            }

            return Task.FromResult(0);
        }
    }
}
