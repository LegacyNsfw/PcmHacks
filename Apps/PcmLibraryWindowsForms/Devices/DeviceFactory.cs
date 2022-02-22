using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    public class DeviceFactory
    {
        /// <summary>
        /// This might not really need to be async. If the J2534 stuff doesn't need it, then this doesn't need it either.
        /// </summary>
        public static Device CreateDeviceFromConfigurationSettings(ILogger logger)
        {
            switch(DeviceConfiguration.Settings.DeviceCategory)
            {
                case DeviceConfiguration.Constants.DeviceCategorySerial:
                    return CreateSerialDevice(DeviceConfiguration.Settings.SerialPort, DeviceConfiguration.Settings.SerialPortDeviceType, logger);

                case DeviceConfiguration.Constants.DeviceCategoryJ2534:
                    return CreateJ2534Device(DeviceConfiguration.Settings.J2534DeviceType, logger);

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
                else
                {
                    port = new StandardPort(serialPortName);
                }

                Device device;
                switch (serialPortDeviceType)
                {
                    case OBDXProDevice.DeviceType:
                        device = new OBDXProDevice(port, logger);
                        break;

                    case AvtDevice.DeviceType:
                        device = new AvtDevice(port, logger);
                        break;

                    case MockDevice.DeviceType:
                        device = new MockDevice(port, logger);
                        break;

                    case ElmDevice.DeviceType:
                        device = new ElmDevice(port, logger);
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

        public static Device CreateJ2534Device(string deviceType, ILogger logger)
        {
            foreach(var device in J2534DeviceFinder.FindInstalledJ2534DLLs(logger))
            {
                if (device.Name == deviceType)
                {
                    return new J2534Device(device, logger);
                }
            }

            return null;
        }
    }
}
