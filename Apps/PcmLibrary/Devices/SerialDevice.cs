using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// Base class for serial-port devices.
    /// </summary>
    public abstract class SerialDevice : Device
    {
        /// <summary>
        /// The serial port this device will use.
        /// </summary>
        protected IPort Port { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public SerialDevice(IPort port, ILogger logger) : base(logger)
        {
            this.Port = port;
        }

        /// <summary>
        /// Disposer.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Port != null)
                {
                    this.Port.Dispose();
                }
            }
        }

        /// <summary>
        /// Generate a descriptive string for this device and the port that it is using.
        /// </summary>
        public override string ToString()
        {
            return this.GetDeviceType() + " on " + this.Port.ToString();
        }

        /// <summary>
        /// Return a descriptive string for this type of hardware.
        /// </summary>
        public abstract string GetDeviceType();
    }
}
