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
                return ConfigurationManager.AppSettings[Constants.DeviceCategorySetting];
            }

            set
            {
                ConfigurationManager.AppSettings[Constants.DeviceCategorySetting] = value;
            }
        }

        public static string SerialPort
        {
            get
            {
                return ConfigurationManager.AppSettings[Constants.SerialPortSetting];
            }

            set
            {
                ConfigurationManager.AppSettings[Constants.SerialPortSetting] = value;
            }
        }

        public static string SerialPortDeviceType
        {
            get
            {
                return ConfigurationManager.AppSettings[Constants.SerialPortDeviceTypeSetting];
            }

            set
            {
                ConfigurationManager.AppSettings[Constants.SerialPortDeviceTypeSetting] = value;
            }
        }

        public static string J2534DeviceType
        {
            get
            {
                return ConfigurationManager.AppSettings[Constants.J2534DeviceTypeSetting];
            }

            set
            {
                ConfigurationManager.AppSettings[Constants.J2534DeviceTypeSetting] = value;
            }
        }
    }
}
