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

                this.LoadParameters<PidParameter>(xml);
                this.LoadParameters<RamParameter>(xml);
                this.LoadParameters<MathParameter>(xml);
            }
            catch(Exception exception)
            {
                this.logger.AddUserMessage("Unable to load profile " + Path.GetFileName(path));
                this.logger.AddDebugMessage(exception.ToString());
                this.profile = new LogProfile();
            }

            return profile;
        }

        private void LoadParameters<T>(XDocument xml) where T : Parameter
        {
            string parameterType = typeof(T).Name;

            XElement container = xml.Root.Elements(string.Format("{0}s", parameterType)).FirstOrDefault();

            if (container != null)
            {
                foreach (XElement parameterElement in container.Elements(parameterType))
                {
                    string id = parameterElement.Attribute("id").Value;
                    string units = parameterElement.Attribute("units").Value;
                    this.AddParameterToProfile<T>(id, units);
                }
            }
        }

        private void AddParameterToProfile<T>(string id, string units) where T : Parameter
        {
            if (!this.database.TryGetParameter<T>(id, out T parameter))
            {
                return;
            }

            if (!parameter.IsSupported(this.osid))
            {
                return;
            }

            if (!parameter.TryGetConversion(units, out Conversion conversion))
            {
                conversion = parameter.Conversions.First();
            }

            LogColumn column = new LogColumn(parameter, conversion);

            this.profile.AddColumn(column);
        }
    }
}
