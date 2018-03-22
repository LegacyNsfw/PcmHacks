using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    abstract class SerialDevice : Device
    {
        public SerialDevice(IPort port, ILogger logger) : base(port, logger)
        {

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
