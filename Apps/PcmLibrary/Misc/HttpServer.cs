using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// Exposes a serial port over an HTTP connection.
    /// </summary>
    /// <remarks>
    /// See the HttpPort class for more info.
    /// This hasn't been used in a while, but it was useful in the beginning.
    /// Not sure if it should be maintained, revived, or just eliminated.
    /// </remarks>
    class HttpServer
    {
        /// <summary>
        /// Singleton.
        /// </summary>
        private static HttpServer instance;

        /// <summary>
        /// Which local serial port to expose remotely.
        /// </summary>
        private IPort port;

        /// <summary>
        /// Provides access to the Results and Debug panes.
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// Listens on the local HTTP port.
        /// </summary>
        private HttpListener listener;

        /// <summary>
        /// Creates an instance of the HttpServer class, and runs it on a background thread.
        /// </summary>
        public static void StartWebServer(IPort port, ILogger logger)
        {
            if (instance != null)
            {
                instance.Close();
                instance = null;
            }

            instance = new HttpServer(port, logger);

            System.Threading.ThreadPool.QueueUserWorkItem(instance.Run);

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public HttpServer(IPort port, ILogger logger)
        {
            this.port = port;
            this.logger = logger;
        }

        /// <summary>
        /// Seemed like a good idea at the time, but there's no UI for it...
        /// </summary>
        public void Close()
        {
            this.listener.Close();
        }

        /// <summary>
        /// Run the HTTP server. Just handle requests as they come in.
        /// </summary>
        public async void Run(object unused)
        {
            // This loop restarts the server if it crashes.
            while (true)
            {
                try
                {
                    this.listener = new HttpListener();
                    this.listener.Prefixes.Add("http://*:11411/pcm/");
                    this.listener.Start();

                    SerialPortConfiguration configuration = new SerialPortConfiguration();
                    configuration.BaudRate = 115200;
                    await this.port.OpenAsync(configuration);

                    // This loop handles incoming connections.
                    while (true)
                    {
                        var context = await this.listener.GetContextAsync();

                        context.Response.ContentType = "text/plain";

                        try
                        {
                            string path = context.Request.Url.AbsolutePath;

                            if (path == "/pcm/send")
                            {
                                await this.Send(context);
                            }
                            else if (path == "/pcm/receive")
                            {
                                await this.Receive(context);
                            }
                            else
                            {
                                var writer = new StreamWriter(context.Response.OutputStream);
                                await writer.WriteLineAsync("Unsupported URL path");
                                context.Response.StatusCode = 404;
                            }
                        }
                        catch (TimeoutException)
                        {
                            context.Response.StatusCode = 504;
                        }
                        catch (Exception exception)
                        {
                            this.logger.AddUserMessage("HTTP 500 " + exception.ToString());

                            var writer = new StreamWriter(context.Response.OutputStream);
                            context.Response.StatusCode = 500;
                            await writer.WriteLineAsync(exception.ToString());
                            await writer.FlushAsync();                            
                        }

                        context.Response.Close();
                    }
                }
                catch (Exception exception)
                {
                    this.logger.AddUserMessage("The web server crashed. Will restart in 5 seconds.");
                    this.logger.AddUserMessage(exception.ToString());

                    if (this.listener != null)
                    {
                        this.listener.Close();
                        this.listener = null;
                    }

                    if (this.port != null)
                    {
                        this.port.Dispose();
                        this.port = null;
                    }

                    await Task.Delay(5000);
                }
            }
        }

        /// <summary>
        /// Send bytes from the HTTP client out the local serial port.
        /// </summary>
        private async Task Send(HttpListenerContext context)
        {
            var queryString = context.Request.QueryString;
            string requestHex = queryString["request"];

            if (!requestHex.IsHex())
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }

            this.logger.AddUserMessage("HTTP request: " + requestHex);

            byte[] bytes = requestHex.ToBytes();

            await port.Send(bytes);

            // Uncomment for testing.
            //
            // await Task.Delay(250);
            // await this.Receive(context);
        }

        /// <summary>
        /// Receive bytes from the serial port and relay them to the client.
        /// </summary>
        private async Task Receive(HttpListenerContext context)
        {
            byte[] buffer = new byte[10 * 1000];
            int length = await port.Receive(buffer, 0, buffer.Length);
            string responseHex = buffer.ToHex(length);

            this.logger.AddUserMessage("HTTP response: " + responseHex);

            var writer = new StreamWriter(context.Response.OutputStream);
            await writer.WriteLineAsync(responseHex);
            await writer.FlushAsync();
        }
    }
}