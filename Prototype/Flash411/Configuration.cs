using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    public static class Configuration
    {
        public class Constants
        {
            public const string DeviceCategorySetting = "DeviceCategory";

            public const string SerialPortSetting = "SerialPort";

            public const string SerialPortDeviceTypeSetting = "SerialPortDeviceType";

            public const string J2534DeviceTypeSetting = "J2534DeviceType";

            public const string DeviceCategorySerial = "Serial";

            public const string DeviceCategoryJ2534 = "J2534";
        }

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

        public static string Read(string key)
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            return configuration.AppSettings.Settings[key].Value;
        }

        public static void Write(string key, string value)
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings.Remove(key);
            configuration.AppSettings.Settings.Add(key, value);
            configuration.Save();
        }
    }
}
