using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// Reads and writes configuration settings.
    /// </summary>
    /// <remarks>
    /// Access to underlying configuration storage is implemented separately, 
    /// so that it can be ported to different systems or tested easily.
    /// </remarks>
    public static class Configuration
    {
        /// <summary>
        /// The code that actually reads and writes individual configuration values.
        /// </summary>
        private static ConfigurationAccessor accessor;

        /// <summary>
        /// Allows abstracting the storage for configuration settings.
        /// </summary>
        public abstract class ConfigurationAccessor
        {
            public abstract string Read(string keyName);
            public abstract void Write(string keyName, string value);
        }

        /// <summary>
        /// Configurations setting key names.
        /// </summary>
        public class Constants
        {
            public const string DeviceCategory = "DeviceCategory";

            public const string SerialPort = "SerialPort";

            public const string SerialPortDeviceType = "SerialPortDeviceType";

            public const string J2534DeviceType = "J2534DeviceType";

            public const string DeviceCategorySerial = "Serial";

            public const string DeviceCategoryJ2534 = "J2534";

            public const string Enable4xReadWrite = "Enable4xReadWrite";
        }

        /// <summary>
        /// Device category (Serial, J2534, etc)
        /// </summary>
        public static string DeviceCategory
        {
            get
            {
                return Read(Constants.DeviceCategory);
            }

            set
            {
                Write(Constants.DeviceCategory, value);
            }
        }

        /// <summary>
        /// Serial port name.
        /// </summary>
        public static string SerialPort
        {
            get
            {
                return Read(Constants.SerialPort);
            }

            set
            {
                Write(Constants.SerialPort, value);
            }
        }

        /// <summary>
        /// Serial device type.
        /// </summary>
        public static string SerialPortDeviceType
        {
            get
            {
                return Read(Constants.SerialPortDeviceType);
            }

            set
            {
                Write(Constants.SerialPortDeviceType, value);
            }
        }

        /// <summary>
        /// J2534 device type.
        /// </summary>
        public static string J2534DeviceType
        {
            get
            {
                return Read(Constants.J2534DeviceType);
            }

            set
            {
                Write(Constants.J2534DeviceType, value);
            }
        }

        /// <summary>
        /// J2534 device type.
        /// </summary>
        public static bool Enable4xReadWrite
        {
            get
            {
                string raw = Read(Constants.Enable4xReadWrite);
                bool result;
                if (bool.TryParse(raw, out result))
                {
                    return result;
                }

                return true;
            }

            set
            {
                Write(Constants.Enable4xReadWrite, value.ToString());
            }
        }

        /// <summary>
        /// Set the configuration accessor to use for this process.
        /// </summary>
        public static void SetAccessor(ConfigurationAccessor accessor)
        {
            Configuration.accessor = accessor;
        }

        /// <summary>
        /// Read a single configuration setting.
        /// </summary>
        public static string Read(string key)
        {
            return accessor.Read(key);
        }

        /// <summary>
        /// Write a single configuration setting.
        /// </summary>
        public static void Write(string key, string value)
        {
            accessor.Write(key, value);
        }
    }
}
