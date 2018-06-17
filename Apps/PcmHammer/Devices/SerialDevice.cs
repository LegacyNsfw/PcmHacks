using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    public abstract class SerialDevice : Device
    {
        protected IPort Port { get; private set; }

        public SerialDevice(IPort port, ILogger logger) : base(logger)
        {
            this.Port = port;
        }

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

        public void UpdateAppConfiguration()
        {
            Configuration.DeviceCategory = Configuration.Constants.DeviceCategorySerial;
            Configuration.SerialPort = this.Port.ToString();
            Configuration.SerialPortDeviceType = this.GetDeviceType();
        }

        public override string ToString()
        {
            return this.GetDeviceType() + " on " + this.Port.ToString();

        }

        public abstract string GetDeviceType();
    }
}
