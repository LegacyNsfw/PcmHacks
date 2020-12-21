using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace PcmHacking
{
    public class ParameterDatabase
    {
        public class ParameterTable<T> where T : Parameter
        {
            private Dictionary<string, T> dictionary = new Dictionary<string, T>();
            public void Add(string id, T parameter)
            {
                this.dictionary[id] = parameter;
    }

            public bool TryGetParameter(string id, out T parameter)
    {
                return this.dictionary.TryGetValue(id, out parameter);
            }
        }

        private string pathToXmlDirectory;
        private List<Parameter> parameters;

        public IEnumerable<Parameter> Parameters { get { return this.parameters; } }
        public ParameterTable<PidParameter> PidParameters { get; private set; }
        public ParameterTable<RamParameter> RamParameters { get; private set; }
        public ParameterTable<MathParameter> MathParameters { get; private set; }

        public ParameterDatabase(string pathToXmlDirectory)
    {
            this.pathToXmlDirectory = pathToXmlDirectory;
            this.PidParameters = new ParameterTable<PidParameter>();
            this.RamParameters = new ParameterTable<RamParameter>();
            this.MathParameters = new ParameterTable<MathParameter>();
        }

        public bool TryLoad(out string errorMessage)
        {
            if (!this.TryLoadStandardParameters(out errorMessage))
        {
                return false;
        }

            if (!this.TryLoadRamParameters(out errorMessage))
        {
                return false;
        }

            if (!this.TryLoadMathParameters(out errorMessage))
        {
                return false;
        }

            return true;
    }

        private bool TryLoadStandardParameters(out string errorMessage)
    {
            string pathToXml = Path.Combine(this.pathToXmlDirectory, "Parameters.Standard.xml");
            XDocument xml = XDocument.Load(pathToXml);
            this.parameters = new List<Parameter>();
            foreach (XElement parameterElement in xml.Root.Elements("Parameter"))
            {
                string parameterName = null;
                try
        {
                    List<Conversion> conversions = new List<Conversion>();
                    foreach (XElement conversion in parameterElement.Elements("Conversion"))
                    {
                        conversions.Add(
                            new Conversion(
                                conversion.Attribute("units").Value,
                                conversion.Attribute("expression").Value,
                                conversion.Attribute("format").Value));
        }

                    string parameterType = (string)parameterElement.Attribute("type");
                    parameterName = (string)parameterElement.Attribute("name").Value;
                    if (parameterType == "PID")
        {
                        PidParameter parameter = new PidParameter(
                            UnsignedHex.GetUnsignedHex("0x" + parameterElement.Attribute("id").Value),
                            parameterName,
                            parameterElement.Attribute("description").Value,
                            int.Parse(parameterElement.Attribute("byteCount").Value),
                            bool.Parse(parameterElement.Attribute("bitMapped").Value),
                            conversions);

                        parameters.Add(parameter);
                        this.PidParameters.Add(parameter.Id, parameter);
                    }
                }
                catch (Exception exception)
                {
                    errorMessage =
                        string.Format("Error in parameter '{0}'{1}{2}",
                        parameterName,
                        Environment.NewLine,
                        exception.ToString());
                    return false;
        }
    }

            errorMessage = null;
            return true;
        }

        private bool TryLoadRamParameters(out string errorMessage)
        {
            string pathToXml = Path.Combine(this.pathToXmlDirectory, "Parameters.RAM.xml");
            XDocument xml = XDocument.Load(pathToXml);
            List<Parameter> ramParameters = new List<Parameter>();
            foreach (XElement parameterElement in xml.Root.Elements("Parameter"))
            {
                string parameterName = null;
                try
    {
                    List<Conversion> conversions = new List<Conversion>();
                    foreach (XElement conversion in parameterElement.Elements("Conversion"))
                    {
                        conversions.Add(
                            new Conversion(
                                conversion.Attribute("units").Value,
                                conversion.Attribute("expression").Value,
                                conversion.Attribute("format").Value));
                    }

                    string parameterType = (string)parameterElement.Attribute("type");
                    parameterName = (string)parameterElement.Attribute("name").Value;
                    if (parameterType == "RAM")
                    {
                        // TODO: Read OSID->address dictionary
                        Dictionary<uint, uint> addresses = new Dictionary<uint, uint>();

                        RamParameter parameter = new RamParameter(
                            parameterElement.Attribute("id").Value,
                            parameterName,
                            parameterElement.Attribute("description").Value,
                            int.Parse(parameterElement.Attribute("byteCount").Value),
                            bool.Parse(parameterElement.Attribute("bitMapped").Value),
                            conversions,
                            addresses);

                        ramParameters.Add(parameter);
                        this.RamParameters.Add(parameter.Id, parameter);
                    }
                }
                catch (Exception exception)
                {
                    errorMessage =
                        string.Format("Error in parameter '{0}'{1}{2}",
                        parameterName,
                        Environment.NewLine,
                        exception.ToString());
                    return false;
                }
            }

            this.parameters.AddRange(ramParameters);
            errorMessage = null;
            return true;
        }

        private bool TryLoadMathParameters(out string errorMessage)
        {
            // TODO
            errorMessage = null;
            return true;
        }
    }
}
