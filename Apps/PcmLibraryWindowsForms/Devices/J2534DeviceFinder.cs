using J2534;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    class J2534DeviceFinder
    {
        private const string PASSTHRU_REGISTRY_PATH = "Software\\PassThruSupport.04.04";
        private const string PASSTHRU_REGISTRY_PATH_6432 = "Software\\Wow6432Node\\PassThruSupport.04.04";

        /// <summary>
        /// Find all installed J2534 DLLs
        /// </summary>
        public static List<J2534.J2534Device> FindInstalledJ2534DLLs(ILogger logger)
        {
            List<J2534.J2534Device> installedDLLs = new List<J2534.J2534Device>();

            try
            {
                RegistryKey myKey = Registry.LocalMachine.OpenSubKey(PASSTHRU_REGISTRY_PATH, false);
                if ((myKey == null))
                {
                    myKey = Registry.LocalMachine.OpenSubKey(PASSTHRU_REGISTRY_PATH_6432, false);
                    if ((myKey == null))
                    {
                        return installedDLLs;
                    }

                }

                string[] devices = myKey.GetSubKeyNames();
                foreach (string device in devices)
                {
                    J2534.J2534Device tempDevice = new J2534.J2534Device();
                    RegistryKey deviceKey = myKey.OpenSubKey(device);
                    if ((deviceKey == null))
                    {
                        continue; //Skip device... its empty
                    }

                    tempDevice.Vendor = (string)deviceKey.GetValue("Vendor", "");
                    tempDevice.Name = (string)deviceKey.GetValue("Name", "");
                    tempDevice.ConfigApplication = (string)deviceKey.GetValue("ConfigApplication", "");
                    tempDevice.FunctionLibrary = (string)deviceKey.GetValue("FunctionLibrary", "");
                    tempDevice.CAN = (int)(deviceKey.GetValue("CAN", 0));
                    tempDevice.ISO14230 = (int)(deviceKey.GetValue("ISO14230", 0));
                    tempDevice.ISO15765 = (int)(deviceKey.GetValue("ISO15765", 0));
                    tempDevice.ISO9141 = (int)(deviceKey.GetValue("ISO9141", 0));
                    tempDevice.J1850PWM = (int)(deviceKey.GetValue("J1850PWM", 0));
                    tempDevice.J1850VPW = (int)(deviceKey.GetValue("J1850VPW", 0));
                    tempDevice.SCI_A_ENGINE = (int)(deviceKey.GetValue("SCI_A_ENGINE", 0));
                    tempDevice.SCI_A_TRANS = (int)(deviceKey.GetValue("SCI_A_TRANS", 0));
                    tempDevice.SCI_B_ENGINE = (int)(deviceKey.GetValue("SCI_B_ENGINE", 0));
                    tempDevice.SCI_B_TRANS = (int)(deviceKey.GetValue("SCI_B_TRANS", 0));
                    installedDLLs.Add(tempDevice);
                }
                return installedDLLs;
            }
            catch (Exception exception)
            {
                logger.AddDebugMessage("Error occured while finding installed J2534 devices");
                logger.AddDebugMessage(exception.ToString());
                return installedDLLs;
            }
        }
    }
}
