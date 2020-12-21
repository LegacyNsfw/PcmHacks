using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PcmHacking
{
    public class LogProfileReader
    {
        private ParameterDatabase database;
        private LogProfile profile;
        private ILogger logger;

        public LogProfileReader(ParameterDatabase database, ILogger logger)
        {
            this.database = database;
            this.profile = new LogProfile();
            this.logger = logger;
        }

        public LogProfile Read(string path)
        {
            try
            {
                XDocument xml = XDocument.Load(path);

                this.LoadPidParameters(xml);
                this.LoadRamParameters(xml);
                this.LoadMathParameters(xml);
            }
            catch(Exception exception)
            {
                this.logger.AddUserMessage("Unable to load profile " + Path.GetFileName(path));
                this.logger.AddDebugMessage(exception.ToString());
                this.profile = new LogProfile();
            }

            return profile;
        }

        private void LoadPidParameters(XDocument xml)
        {
            XElement container = xml.Root.Elements("PidParameters").FirstOrDefault();
            if (container != null)
            {
                foreach (XElement parameterElement in container.Elements("PidParameter"))
                {
                    string id = parameterElement.Attribute("id").Value;
                    string units = parameterElement.Attribute("units").Value;
                    this.AddPidParameter(id, units);
                }
            }
        }

        private void LoadRamParameters(XDocument xml)
        {
            XElement container = xml.Root.Elements("RamParameters").FirstOrDefault();
            if (container != null)
            {
                foreach (XElement parameterElement in container.Elements("RamParameter"))
                {
                    string id = parameterElement.Attribute("id").Value;
                    string units = parameterElement.Attribute("units").Value;
                    this.AddRamParameter(id, units);
                }
            }
        }

        private void LoadMathParameters(XDocument xml)
        {
            XElement container = xml.Root.Elements("MathParameters").FirstOrDefault();
            if (container != null)
            {
                foreach (XElement parameterElement in container.Elements("MathParameter"))
                {
                    string id = parameterElement.Attribute("id").Value;
                    string units = parameterElement.Attribute("units").Value;
                    this.AddMathParameter(id, units);
                }
            }
        }

        private void AddPidParameter(string id, string units)
        {
            PidParameter parameter;
            if (!this.database.PidParameters.TryGetParameter(id, out parameter))
            {
                return;
            }

            Conversion conversion;
            if (!parameter.TryGetConversion(units, out conversion))
            {
                conversion = parameter.Conversions.First();
            }

            ProfileParameter profileParameter = new ProfileParameter(parameter, conversion);
            this.profile.AddParameter(profileParameter);
        }

        private void AddRamParameter(string id, string units)
        {
        }

        private void AddMathParameter(string id, string units)
        {
        }
    }
}
