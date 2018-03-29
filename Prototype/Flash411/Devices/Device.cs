using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// The Interface classes are responsible for commanding a hardware
    /// interface to send and receive VPW messages.
    /// 
    /// They use the IPort interface to communicate with the hardware.
    /// TODO: Move the IPort stuff into the SerialDevice class, since J2534 devices don't need it.
    /// </summary>
    abstract class Device : IDisposable
    {
        protected IPort Port { get; private set; }

        protected ILogger Logger { get; private set; }

        public int MaxSendSize { get; protected set; }

        public int MaxReceiveSize { get; protected set; }

        public bool Supports4X { get; protected set; }

        public Device(IPort port, ILogger logger)
        {
            this.Port = port;
            this.Logger = logger;

            // These default values can be overwritten in derived classes.
            this.MaxSendSize = 100;
            this.MaxReceiveSize = 100;
            this.Supports4X = false;
        }

        public Device(ILogger logger)
        {
            this.Logger = logger;
        }

        ~Device()
        {
            this.Dispose(false);
        }

        public virtual void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Close();
        }

        public abstract Task<bool> Initialize();

        protected virtual void Close()
        {
            if (this.Port != null)
            {
                this.Port.Dispose();
            }
        }

        /// <summary>
        /// Send a message, do not expect a response.
        /// </summary>
        public abstract Task<bool> SendMessage(Message message);

        /// <summary>
        /// Send a message, wait for a response, return the response.
        /// </summary>
        public abstract Task<Response<Message>> SendRequest(Message message);
    }
}
