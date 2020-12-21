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

        public LogProfileReader(ParameterDatabase database)
        {
            this.database = database;
            this.profile = new LogProfile();
        }

        public LogProfile Read(string path)
        {
            XDocument xml = XDocument.Load(path);

            this.LoadPidParameters(xml);
            this.LoadRamParameters(xml);
            this.LoadMathParameters(xml);

            return profile;
        }

        private void LoadPidParameters(XDocument xml)
        {
            foreach (XElement parameterElement in xml.Root.Elements("PidParameter"))
            {
                string id = parameterElement.Attribute("id").Value;
                string units = parameterElement.Attribute("units").Value;
                this.AddPidParameter(id, units);
            }
        }

        private void LoadRamParameters(XDocument xml)
        {
            foreach (XElement parameterElement in xml.Root.Elements("RamParameter"))
            {
                string id = parameterElement.Attribute("id").Value;
                string units = parameterElement.Attribute("units").Value;
                this.AddRamParameter(id, units);
            }
        }

        private void LoadMathParameters(XDocument xml)
        {
            foreach (XElement parameterElement in xml.Root.Elements("MathParameter"))
            {
                string id = parameterElement.Attribute("id").Value;
                string units = parameterElement.Attribute("units").Value;
                this.AddMathParameter(id, units);
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
