using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    class WinFormsConfigurationAccessor : Configuration.ConfigurationAccessor
    {
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
