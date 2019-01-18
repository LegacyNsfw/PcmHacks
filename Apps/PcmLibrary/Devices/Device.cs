using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// What the code was doing when a timeout happened.
    /// </summary>
    public enum TimeoutScenario
    {
        Undefined = 0,
        ReadProperty,
        ReadCrc,
        SendKernel,
        ReadMemoryBlock,
        Maximum,
    }

    /// <summary>
    /// VPW can operate at two speeds. It is generally in standard (low speed) mode, but can switch to 4X (high speed).
    /// </summary>
    /// <remarks>
    /// High speed is better whend reading the entire contents of the PCM.
    /// Transitions to high speed must be negotiated, and any module that doesn't
    /// want to switch can force the bus to stay at standard speed. Annoying.
    /// </remarks>
    public enum VpwSpeed
    {
        /// <summary>
        /// 10.4 kpbs. This is the standard VPW speed.
        /// </summary>
        Standard,

        /// <summary>
        /// 41.2 kbps. This is the high VPW speed.
        /// </summary>
        FourX,
    }

    /// <summary>
    /// The Interface classes are responsible for commanding a hardware
    /// interface to send and receive VPW messages.
    /// 
    /// They use the IPort interface to communicate with the hardware.
    /// TODO: Move the IPort stuff into the SerialDevice class, since J2534 devices don't need it.
    /// </summary>
    public abstract class Device : IDisposable
    {
        private int maxSendSize;

        /// <summary>
        /// Provides access to the Results and Debug panes.
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Maximum size of sent messages.
        /// </summary>
        /// <remarks>
        /// Max send size is currently limited to 2k, because the kernel
        /// crashes at startup with a 4k buffer.
        /// TODO: Make the kernel happy with a 4k buffer, remove this limit.
        /// </remarks>
        public int MaxSendSize
        {
            get
            {
                return Math.Min(2048+12, this.maxSendSize);
            }

            protected set
            {
                this.maxSendSize = value;
            }
        }

        /// <summary>
        /// Maximum size of received messages.
        /// </summary>
        public int MaxReceiveSize { get; protected set; }

        /// <summary>
        /// Indicates whether or not the device supports 4X speed.
        /// </summary>
        public bool Supports4X { get; protected set; }

        /// <summary>
        /// Number of messages recevied so far.
        /// </summary>
        public int ReceivedMessageCount { get { return this.queue.Count; } }

        /// <summary>
        /// Queue of messages received from the VPW bus.
        /// </summary>
        private Queue<Message> queue = new Queue<Message>();

        /// <summary>
        /// Current speed of the VPW bus.
        /// </summary>
        private VpwSpeed speed;

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
            this.speed = VpwSpeed.Standard;
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
        /// Set the timeout period to wait for responses to incoming messages.
        /// </summary>
        public abstract Task SetTimeout(TimeoutScenario scenario);

        /// <summary>
        /// Send a message.
        /// </summary>
        public abstract Task<bool> SendMessage(Message message);

        /// <summary>
        /// Removes any messages that might be waiting in the incoming-message queue. Also clears the buffer.
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
        /// Set the device's VPW data rate.
        /// </summary>
        public async Task<bool> SetVpwSpeed(VpwSpeed newSpeed)
        {
            if (this.speed == newSpeed)
            {
                return true;
            }

            if (!await this.SetVpwSpeedInternal(newSpeed))
            {
                return false;
            }

            this.speed = newSpeed;
            return true;
        }

        /// <summary>
        /// Set the interface to low (false) or high (true) speed
        /// </summary>
        protected abstract Task<bool> SetVpwSpeedInternal(VpwSpeed newSpeed);

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

        /// <summary>
        /// List for an incoming message of the VPW bus.
        /// </summary>
        protected abstract Task Receive();

        /// <summary>
        /// Calculates the time required for the given scenario at the current VPW speed.
        /// </summary>
        protected int GetVpwTimeoutMilliseconds(TimeoutScenario scenario)
        {
            int packetSize;

            switch (scenario)
            {
                case TimeoutScenario.ReadProperty:
                    // Approximate number of bytes in a get-VIN or get-OSID response.
                    packetSize = 50;
                    break;

                case TimeoutScenario.ReadCrc:
                    // These packets are actually only 15 bytes, but the ReadProperty timeout wasn't
                    // enough for the AllPro at 4x. Still not sure why. So this is a bit of a hack.
                    // TODO: Figure out why the AllPro needs a hack to receive CRC values at 4x.
                    packetSize = 1000;
                    break;

                case TimeoutScenario.ReadMemoryBlock:
                    // Adding 20 bytes to account for the 'read request accepted' 
                    // message that comes before the read payload.
                    packetSize = 20 + this.MaxReceiveSize;

                    // Not sure why this is necessary, but AllPro 2k reads won't work without it.
                    //packetSize = (int) (packetSize * 1.1);
                    packetSize = (int) (packetSize * 2.5);
                    break;

                case TimeoutScenario.SendKernel:
                    packetSize = this.MaxSendSize + 20;
                    break;

                case TimeoutScenario.Maximum:
                    return 0xFF * 4; 

                default:
                    throw new NotImplementedException("Unknown timeout scenario " + scenario);
            }

            int bitsPerByte = 9; // 8N1 serial
            double bitsPerSecond = this.speed == VpwSpeed.Standard ? 10.4 : 41.6;
            double milliseconds = (packetSize * bitsPerByte) / bitsPerSecond;

            // Add 10% just in case.
            return (int)(milliseconds * 1.1);
        }

        /// <summary>
        /// Estimate timeouts. The code above seems to do a pretty good job, but this is easier to experiment with.
        /// </summary>
        protected int __GetVpwTimeoutMilliseconds(TimeoutScenario scenario)
        {
            switch (scenario)
            {
                case TimeoutScenario.ReadProperty:
                    return 100;

                case TimeoutScenario.ReadMemoryBlock:
                    return 2500;

                case TimeoutScenario.SendKernel:
                    return 1000;

                default:
                    throw new NotImplementedException("Unknown timeout scenario " + scenario);
            }
        }
    }
}
