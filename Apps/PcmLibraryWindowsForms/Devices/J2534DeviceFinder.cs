using J2534DotNet;
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
        /// <summary>
        /// Find all installed J2534 DLLs
        /// </summary>
        public static List<J2534DotNet.J2534Device> FindInstalledJ2534DLLs(ILogger logger)
        {
            List<J2534DotNet.J2534Device> installedDLLs = new List<J2534DotNet.J2534Device>();
            try
            {
                installedDLLs = J2534DotNet.J2534Detect.ListDevices();
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
