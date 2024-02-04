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
        Minimum,
        ReadProperty,
        ReadCrc,
        SendKernel,
        ReadMemoryBlock,
        EraseMemoryBlock,
        WriteMemoryBlock,
        DataLogging1,
        DataLogging2,
        DataLogging3,
        DataLogging4,
        DataLoggingStreaming,
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
        /// <summary>
        /// Max transmit size.
        /// </summary>
        private int maxSendSize;

        /// <summary>
        /// Queue of messages received from the VPW bus.
        /// </summary>
        private Queue<Message> queue = new Queue<Message>();

        /// <summary>
        /// For the AllPro, we need to tell the interface how long to listen for incoming messages.
        /// For other devices this is not so critical, however I suspect it might still be useful to set serial-port timeouts.
        /// </summary>
        protected TimeoutScenario currentTimeoutScenario = TimeoutScenario.Undefined;

        /// <summary>
        /// Provides access to the Results and Debug panes.
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Maximum size of sent messages.
        /// </summary>
        /// <remarks>
        /// This is protected, not public, because consumers should use MaxKernelSendSize or MaxFlashWriteSendSize.
        /// </remarks>
        protected int MaxSendSize
        {
            get
            {
                return this.maxSendSize;
            }

            set
            {
                this.maxSendSize = value;
            }
        }

        /// <summary>
        /// Maximum packet size when sending the kernel.
        /// </summary>
        /// <remarks>
        /// For the P04 PCM, the kernel must be sent in a single message,
        /// so the constraint that we use for flash writing needs to be 
        /// ignored for that request.
        /// </remarks>
        public int MaxKernelSendSize
        {
            get
            {
                return this.maxSendSize;
            }
        }

        /// <summary>
        /// Maximum packet size when writing to flash memory.
        /// </summary>
        /// <remarks>
        /// This is smaller than the device's actual max send size, for 3 reasons:
        /// 1) P01/P59 kernels will currently crash with large writes.
        /// 2) Large packets are increasingly susceptible to noise corruption on the VPW bus.
        /// 3) Even on a clean VPW bus, the speed increases negligibly above 2kb.
        /// </remarks>
        public int MaxFlashWriteSendSize
        {
            get
            {
                return Math.Min(1024 + 12, this.maxSendSize);
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
        /// Indicates whether or no the device supports logging just one DPID.
        /// </summary>
        /// <remarks>
        /// ELM devices generally need at least two DPIDs to work. Apparently if
        /// there is only one DPID, it comes back from the PCM before the device 
        /// is ready to listen. So the device times out, and logging is broken.
        /// </remarks>
        public bool SupportsSingleDpidLogging { get; protected set; }

        /// <summary>
        /// Indicates whether or not the device supports stream data logging.
        /// </summary>
        /// <remarks>
        /// The default approach to logging uses one message from the app to request
        /// one row of data from the PCM.
        /// 
        /// The stream approach causes the PCM to send a continuous stream of data 
        /// after the inital "start logging" request, which allows it to send 
        /// 25%-50% more rows per second.
        /// </remarks>
        public bool SupportsStreamLogging { get; protected set; }

        /// <summary>
        /// Number of messages recevied so far.
        /// </summary>
        public int ReceivedMessageCount { get { return this.queue.Count; } }

        /// <summary>
        /// Gets the number of messages waiting in the receive queue.
        /// </summary>
        /// <remarks>
        /// Probably only useful for debug messages.
        /// </remarks>
        protected int QueueSize { get { return this.queue.Count; } }

        /// <summary>
        /// Current speed of the VPW bus.
        /// </summary>
        protected VpwSpeed Speed { get; private set; }

        /// <summary>
        /// Enable Disable VPW 4x.
        /// </summary>
        public bool Enable4xReadWrite { get; set; }

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
            this.Speed = VpwSpeed.Standard;
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
        public abstract Task<TimeoutScenario> SetTimeout(TimeoutScenario scenario);

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
                    // This can be useful for debugging, but is generally too noisy.
                    // this.Logger.AddDebugMessage("Dequeue.");
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
            if (this.Speed == newSpeed)
            {
                return true;
            }

            if (((newSpeed == VpwSpeed.FourX) && !this.Enable4xReadWrite) || (!await this.SetVpwSpeedInternal(newSpeed)))
            {
                return false;
            }

            this.Speed = newSpeed;
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
                this.Logger.AddDebugMessage("Received: " + message.ToString());
                this.queue.Enqueue(message);
            }
        }

        /// <summary>
        /// List for an incoming message of the VPW bus.
        /// </summary>
        protected abstract Task Receive();

        /// <summary>
        /// This is no longer used, but is being kept for now since the comments
        /// shed some light on the differences between AllPro and Scantool LX 
        /// (probably not SX) interfaces.
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
                    packetSize = this.MaxKernelSendSize + 20;
                    break;

                // Not tuned manually yet.
                case TimeoutScenario.DataLogging1:
                    packetSize = 30;
                    break;

                // This one was tuned by hand to avoid timeouts with STPX, and it work well for the AllPro too.
                case TimeoutScenario.DataLogging2:
                    packetSize = 47;
                    break;

                // 64 works for the LX, but the AllPro needs 70.
                case TimeoutScenario.DataLogging3:
                    packetSize = 70;
                    break;

                // TODO: Tune.
                case TimeoutScenario.DataLogging4:
                    packetSize = 90;
                    break;

                // TODO: Tune.
                case TimeoutScenario.DataLoggingStreaming:
                    packetSize = 0;
                    break;

                case TimeoutScenario.Maximum:
                    return 0xFF * 4; 

                default:
                    throw new NotImplementedException("Unknown timeout scenario " + scenario);
            }

            int bitsPerByte = 9; // 8N1 serial
            double bitsPerSecond = this.Speed == VpwSpeed.Standard ? 10.4 : 41.6;
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
