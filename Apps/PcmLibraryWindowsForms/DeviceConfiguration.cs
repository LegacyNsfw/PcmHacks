using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    public class DeviceConfiguration
    {
        public static PcmLibraryWindowsForms.Properties.Settings Settings = PcmLibraryWindowsForms.Properties.Settings.Default;

        public class Constants
        {
            public const string DeviceCategorySerial = "Serial";
            public const string DeviceCategoryJ2534 = "J2534";
        }
    }
}
