using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    class DeviceFactory
    {
        /// <summary>
        /// This might not really need to be async. If the J2534 stuff doesn't need it, then this doesn't need it either.
        /// </summary>
        public static async Task<Device> CreateDeviceFromConfigurationSettings(ILogger logger)
        {
            switch(Configuration.DeviceCategory)
            {
                case Configuration.Constants.DeviceCategorySerial:
                    return CreateSerialDevice(Configuration.SerialPort, Configuration.SerialPortDeviceType, logger);

                case Configuration.Constants.DeviceCategoryJ2534:
                    return await CreateJ2534Device(Configuration.J2534DeviceType, logger);

                default:
                    return null;
            }
        }

        public static Device CreateSerialDevice(string serialPortName, string serialPortDeviceType, ILogger logger)
        {
            try
            {
                IPort port;
                if (string.Equals(MockPort.PortName, serialPortName))
                {
                    port = new MockPort(logger);
                }
                else if (string.Equals(HttpPort.PortName, serialPortName))
                {
                    port = new HttpPort(logger);
                }
                {
                    port = new StandardPort(serialPortName);
                }

                Device device;
                switch (serialPortDeviceType)
                {
                    case AvtDevice.DeviceType:
                        device = new AvtDevice(port, logger);
                        break;

                    case MockDevice.DeviceType:
                        device = new MockDevice(port, logger);
                        break;

                    case ScanToolDevice.DeviceType:
                        device = new ScanToolDevice(port, logger);
                        break;

                    case ThanielDevice.DeviceType:
                        device = new ThanielDevice(port, logger);
                        break;

                    default:
                        device = null;
                        break;
                }

                if (device == null)
                {
                    return null;
                }

                return device;
            }
            catch (Exception exception)
            {
                logger.AddUserMessage($"Unable to create {serialPortDeviceType} on {serialPortName}.");
                logger.AddDebugMessage(exception.ToString());
                return null;
            }
        }

        public static Task<Device> CreateJ2534Device(string deviceType, ILogger logger)
        {
            return Task.FromResult((Device)null);
        }
    }
}
