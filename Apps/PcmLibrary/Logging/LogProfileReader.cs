using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PcmHacking
{
    /// <summary>
    /// Reads a LogProfile from an XML file.
    /// </summary>
    public class LogProfileReader
    {
        private readonly ParameterDatabase database;
        private readonly uint osid;
        private readonly ILogger logger;

        private LogProfile profile;

        public LogProfileReader(ParameterDatabase database, uint osid, ILogger logger)
        {
            this.database = database;
            this.osid = osid;            
            this.logger = logger;

            this.profile = new LogProfile();
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

            if (!parameter.IsSupported(this.osid))
            {
                return;
            }

            Conversion conversion;
            if (!parameter.TryGetConversion(units, out conversion))
            {
                conversion = parameter.Conversions.First();
            }

            LogColumn column = new LogColumn(parameter, conversion);
            this.profile.AddColumn(column);
        }

        private void AddRamParameter(string id, string units)
        {
            RamParameter parameter;
            if (!this.database.RamParameters.TryGetParameter(id, out parameter))
            {
                return;
            }

            if (!parameter.IsSupported(this.osid))
            {
                return;
            }

            Conversion conversion;
            if (!parameter.TryGetConversion(units, out conversion))
            {
                conversion = parameter.Conversions.First();
            }

            LogColumn column = new LogColumn(parameter, conversion);
            this.profile.AddColumn(column);
        }

        private void AddMathParameter(string id, string units)
        {
            MathParameter parameter;
            if (!this.database.MathParameters.TryGetParameter(id, out parameter))
            {
                return;
            }

            if (!parameter.IsSupported(this.osid))
            {
                return;
            }

            Conversion conversion;
            if (!parameter.TryGetConversion(units, out conversion))
            {
                conversion = parameter.Conversions.First();
            }

            LogColumn column = new LogColumn(parameter, conversion);
            this.profile.AddColumn(column);
        }
    }
}
