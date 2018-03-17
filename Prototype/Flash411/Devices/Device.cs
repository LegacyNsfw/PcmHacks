﻿using System;
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
    /// </summary>
    abstract class Device : IDisposable
    {
        protected IPort Port { get; private set; }

        protected ILogger Logger { get; private set; }

        public Device(IPort port, ILogger logger)
        {
            this.Port = port;
            this.Logger = logger;
        }

        ~Device()
        {
            this.Dispose(false);
        }

        public void Dispose()
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
