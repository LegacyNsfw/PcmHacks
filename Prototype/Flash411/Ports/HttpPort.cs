using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// This class is just here to enable testing without any actual interface hardware.
    /// </summary>
    /// <remarks>
    /// Eventually the Receive method should return simulated VPW responses.
    /// </remarks>
    class HttpPort : IPort
    {
        public const string PortName = "Http Port";
        private ILogger logger;
        private readonly Uri baseUri;

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
        /// Send bytes to the mock PCM.
        /// </summary>
        async Task IPort.Send(byte[] buffer)
        {
            HttpClient client = new HttpClient();
            Uri requestUri = new Uri(baseUri, "/pcm/send?request=" + buffer.ToHex().Replace(" ", "%20"));
            var response = await client.GetAsync(requestUri);
            this.logger.AddDebugMessage("HttpPort.Send StatusCode: " + response.StatusCode.ToString());
        }

        /// <summary>
        /// Receive bytes from the mock PCM.
        /// </summary>
        async Task<int> IPort.Receive(byte[] buffer, int offset, int count)
        {
            HttpClient client = new HttpClient();
            Uri requestUri = new Uri(baseUri, "/pcm/receive");
            var response = await client.GetAsync(requestUri);
            this.logger.AddDebugMessage("HttpPort.Receive StatusCode: " + response.StatusCode.ToString());

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return 0;
            }

            string body = await response.Content.ReadAsStringAsync();
            if(!body.IsHex())
            {
                this.logger.AddDebugMessage("HttpResponse body is not in hex format.");
                return 0;
            }

            this.logger.AddDebugMessage("Response body: " + body);

            byte[] responseBytes = body.ToBytes();
            int copied = 0;
            for(int index = 0; (index < count) && (index < responseBytes.Length); index++)
            {
                buffer[offset + index] = responseBytes[index];
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
    }
}
