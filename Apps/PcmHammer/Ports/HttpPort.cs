using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// Early in the development process, we used this to test different types of hardware remotely.
    /// </summary>
    /// <remarks>
    /// This never supported anything beyond the get-properties operation, and wasn't
    /// very reliable, but it helped us get going. 
    /// 
    /// The HTTP transaction model works well with send-then-recieve pairs, but not
    /// with send-receieve-receiveagain, which is needed if an unexpected message appears
    /// on the bus. So this would need some work to be reliable enough for full content
    /// reads or writes.
    /// </remarks>
    class HttpPort : IPort
    {
        /// <summary>
        /// Port name for the DevicePicker form and internal comparisons.
        /// </summary>
        public const string PortName = "Http Port";

        /// <summary>
        /// Access to the Results and Debug panes.
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// The URL to connect to.
        /// </summary>
        private readonly Uri baseUri;

        /// <summary>
        /// The response message from the latest request.
        /// </summary>
        private List<byte> responseBytes = new List<byte>();

        /// <summary>
        /// Used only to synchronize access to non-reentrant code.
        /// </summary>
        private object sync = new object();

        /// <summary>
        /// Constructor.
        /// </summary>
        public HttpPort(ILogger logger)
        {
            this.logger = logger;

            // TODO: Get this from a parameter, and get the GUI to provide it.
            this.baseUri = new Uri("http://127.0.0.1:11411/");
        }

        /// <summary>
        /// This returns the string that appears in the drop-down list.
        /// </summary>
        public override string ToString()
        {
            return PortName;
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
        /// Send bytes to the remote PCM.
        /// </summary>
        async Task IPort.Send(byte[] buffer)
        {
            HttpClient client = new HttpClient();
            Uri requestUri = new Uri(baseUri, "/pcm/send?request=" + buffer.ToHex().Replace(" ", "%20"));
            var response = await client.GetAsync(requestUri);
            this.logger.AddDebugMessage("HttpPort.Send StatusCode: " + response.StatusCode.ToString());
        }

        /// <summary>
        /// Receive bytes from the remote PCM.
        /// </summary>
        async Task<int> IPort.Receive(byte[] buffer, int offset, int count)
        {
            List<byte> lastBuffer = null;

            lock (sync)
            {
                lastBuffer = this.responseBytes;
                this.responseBytes = new List<byte>();
            }

            if (lastBuffer.Count == 0)
            {
                await Receive();

                lock (sync)
                {
                    lastBuffer = this.responseBytes;
                    this.responseBytes = new List<byte>();
                }

                if (lastBuffer.Count == 0)
                {
                    return 0;
                }
            }

            int copied = 0;
            for (int index = 0; (index < count) && (index < lastBuffer.Count); index++)
            {
                buffer[offset + index] = lastBuffer[index];
                copied++;
            }

            return copied;
        }

        /// <summary>
        /// Discard anything in the input and output buffers.
        /// </summary>
        public async Task DiscardBuffers()
        {
            HttpClient client = new HttpClient();
            Uri requestUri = new Uri(baseUri, "/pcm/receive");
            var response = await client.GetAsync(requestUri);
            this.logger.AddDebugMessage("HttpPort.DiscardBuffers StatusCode: " + response.StatusCode.ToString());
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
        /// <remarks>
        /// This is hacky:
        /// 
        /// For now, you MUST call this before calling Receive.
        /// Only because I'm too lazy to add support for /pcm/bytesToRead :-)
        /// But that is the right thing to do. Maybe soon.
        /// 
        /// For now, the device code always checks this property first so this will suffice.
        /// </remarks>
        public async Task<int> GetReceiveQueueSize()
        {
            int result = 0;

            // These locks might be pointless. Getting the count is probably thread-safe.
            lock (sync)
            {
                result = this.responseBytes.Count;
            }

            if (result > 0)
            {
                return result;
            }

            await Receive();

            lock (sync)
            {
                result = this.responseBytes.Count;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task Receive()
        {
            HttpClient client = new HttpClient();
            Uri requestUri = new Uri(baseUri, "/pcm/receive");
            var response = await client.GetAsync(requestUri);
            this.logger.AddDebugMessage("HttpPort.Receive StatusCode: " + response.StatusCode.ToString());

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return;
            }

            string body = await response.Content.ReadAsStringAsync();
            if (!body.IsHex())
            {
                this.logger.AddDebugMessage("HttpResponse body is not in hex format.");
                return;
            }

            this.logger.AddDebugMessage("Response body: " + body);

            byte[] buffer = body.ToBytes();

            lock (sync)
            {
                responseBytes.AddRange(buffer);
            }
        }
    }
}
