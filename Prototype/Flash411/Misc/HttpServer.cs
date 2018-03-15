using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    class HttpServer
    {
        private static HttpServer instance;

        private IPort port;
        private ILogger logger;
        private HttpListener listener;

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

        public HttpServer(IPort port, ILogger logger)
        {
            this.port = port;
            this.logger = logger;
        }

        public void Close()
        {
            this.listener.Close();
        }

        public async void Run(object unused)
        {
            // Restart the server if it crashes.
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

                    // Handle incoming connections.
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