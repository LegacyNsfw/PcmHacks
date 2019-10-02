using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// Reads and writes the app.config file, to persist settings between app uses.
    /// </summary>
    public class WinFormsConfigurationAccessor : Configuration.ConfigurationAccessor
    {
        /// <summary>
        /// Create a configuration setting.
        /// </summary>
        public override string Read(string key)
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var setting = configuration.AppSettings.Settings[key];
            if (setting == null)
            {
                return string.Empty;
            }

            return setting.Value;
        }

        /// <summary>
        /// Write a configuration setting.
        /// </summary>
        public override void Write(string key, string value)
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            try
            {
                configuration.AppSettings.Settings.Remove(key);
                configuration.AppSettings.Settings.Add(key, value);
                configuration.Save();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Could not save configuration: '{0}'", e);
            }
        }
    }
}
