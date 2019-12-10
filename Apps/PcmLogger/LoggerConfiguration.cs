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
            public const string LogDirectory = "LogDirectory";
        }

        /// <summary>
        /// Path of the most recent log profile.
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

        /// <summary>
        /// Directory to store log files.
        /// </summary>
        public static string LogDirectory
        {
            get
            {
                return Configuration.Read(LoggerConstants.LogDirectory);
            }

            set
            {
                Configuration.Write(LoggerConstants.LogDirectory, value);
            }
        }
    }
}
