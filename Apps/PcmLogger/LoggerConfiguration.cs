using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    public class LoggerConfiguration
    {
        public class LoggerConstants
        {
            public const string ProfilePath = "ProfilePath";
        }

        /// <summary>
        /// Device category (Serial, J2534, etc)
        /// </summary>
        public static string ProfilePath
        {
            get
            {
                return Configuration.Read(LoggerConstants.ProfilePath);
            }

            set
            {
                Configuration.Write(LoggerConstants.ProfilePath, value);
            }
        }
    }
}
