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
    public abstract class Device : IDisposable
    {
        protected ILogger Logger { get; private set; }

        public int MaxSendSize { get; protected set; }

        public int MaxReceiveSize { get; protected set; }

        public bool Supports4X { get; protected set; }

        public int ReceivedMessageCount { get { return this.queue.Count; } }

        /// <summary>
        /// Queue of messages received from the VPW bus.
        /// </summary>
        private Queue<Message> queue = new Queue<Message>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public Device(ILogger logger)
        {
            this.Logger = logger;

            // These default values can be overwritten in derived classes.
            this.MaxSendSize = 100;
            this.MaxReceiveSize = 100;
            this.Supports4X = false;
        }

        /// <summary>
        /// Finalizer (invoked during garbage collection).
        /// </summary>
        ~Device()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Clean up anything allocated by this instane.
        /// </summary>
        public virtual void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Make the device ready to communicate with the VPW bus.
        /// </summary>
        public abstract Task<bool> Initialize();

        /// <summary>
        /// Send a message.
        /// </summary>
        public abstract Task<bool> SendMessage(Message message);

        /// <summary>
        /// Removes any messages that might be waiting in the incoming-message queue.
        /// </summary>
        public void ClearMessageQueue()
        {
            this.queue.Clear();
            ClearMessageBuffer();
        }
        /// <summary>
        /// Clears Serial port buffer or J2534 api buffer
        /// </summary>
        public abstract void ClearMessageBuffer();


        /// <summary>
        /// Reads a message from the VPW bus and returns it.
        /// </summary>
        public async Task<Message> ReceiveMessage()
        {
            if (this.queue.Count == 0)
            {
                await this.Receive();
            }

            lock (this.queue)
            {
                if (this.queue.Count > 0)
                {
                    return this.queue.Dequeue();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Set the interface to low (false) or high (true) speed
        /// </summary>
        public abstract Task<bool> SetVPW4x(bool highspeed);

        /// <summary>
        /// Clean up anything that this instance has allocated.
        /// </summary>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// Add a received message to the queue.
        /// </summary>
        protected void Enqueue(Message message)
        {
            lock (this.queue)
            {
                this.queue.Enqueue(message);
            }
        }

        protected abstract Task Receive();
    }
}
