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
            public const string DeviceCategorySetting = "DeviceCategory";

            public const string SerialPortSetting = "SerialPort";

            public const string SerialPortDeviceTypeSetting = "SerialPortDeviceType";

            public const string J2534DeviceTypeSetting = "J2534DeviceType";

            public const string DeviceCategorySerial = "Serial";

            public const string DeviceCategoryJ2534 = "J2534";
        }

        /// <summary>
        /// Device category (Serial, J2534, etc)
        /// </summary>
        public static string DeviceCategory
        {
            get
            {
                return Read(Constants.DeviceCategorySetting);
            }

            set
            {
                Write(Constants.DeviceCategorySetting, value);
            }
        }

        /// <summary>
        /// Serial port name.
        /// </summary>
        public static string SerialPort
        {
            get
            {
                return Read(Constants.SerialPortSetting);
            }

            set
            {
                Write(Constants.SerialPortSetting, value);
            }
        }

        /// <summary>
        /// Serial device type.
        /// </summary>
        public static string SerialPortDeviceType
        {
            get
            {
                return Read(Constants.SerialPortDeviceTypeSetting);
            }

            set
            {
                Write(Constants.SerialPortDeviceTypeSetting, value);
            }
        }

        /// <summary>
        /// J2534 device type.
        /// </summary>
        public static string J2534DeviceType
        {
            get
            {
                return Read(Constants.J2534DeviceTypeSetting);
            }

            set
            {
                Write(Constants.J2534DeviceTypeSetting, value);
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
