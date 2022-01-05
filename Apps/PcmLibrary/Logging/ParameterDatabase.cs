using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PcmHacking
{
    /// <summary>
    /// This loads the collection of known parameters from XML files.
    /// </summary>
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

        /// <summary>
        /// Constructor
        /// </summary>
        public ParameterDatabase(string pathToXmlDirectory)
        {
            this.pathToXmlDirectory = pathToXmlDirectory;
            this.PidParameters = new ParameterTable<PidParameter>();
            this.RamParameters = new ParameterTable<RamParameter>();
            this.MathParameters = new ParameterTable<MathParameter>();
        }

        /// <summary>
        /// Load everything.
        /// </summary>
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

        /// <summary>
        /// Load standard parameters.
        /// </summary>
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
                    List<Conversion> conversions = GetConversions(parameterElement);

                    parameterName = (string)parameterElement.Attribute("name").Value;
                    PidParameter parameter = new PidParameter(
                        UnsignedHex.GetUnsignedHex("0x" + parameterElement.Attribute("id").Value),
                        parameterName,
                        parameterElement.Attribute("description").Value,
                        parameterElement.Attribute("storageType").Value,
                        bool.Parse(parameterElement.Attribute("bitMapped").Value),
                        conversions);

                    parameters.Add(parameter);
                    this.PidParameters.Add(parameter.Id, parameter);
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

        List<Conversion> GetConversions(XElement parameterElement)
        {
            List<Conversion> conversions = new List<Conversion>();
            bool bitMapped = GetBitMappedFlag(parameterElement);
            if (bitMapped)
            {
                string bitIndexString = parameterElement.Attribute("bitIndex").Value;
                int bitIndex = int.Parse(bitIndexString);
                foreach (XElement conversion in parameterElement.Elements("Conversion"))
                {
                    conversions.Add(CreateBooleanConversion(conversion, bitIndex));
                }
            }
            else
            {
                foreach (XElement conversion in parameterElement.Elements("Conversion"))
                {
                    conversions.Add(CreateNumericConversion(conversion));
                }
            }

            return conversions;
        }

        private bool GetBitMappedFlag(XElement parameterElement)
        {
            string bitMappedAttributeValue = parameterElement.Attribute("bitMapped")?.Value;
            if (string.IsNullOrEmpty(bitMappedAttributeValue))
            {
                return false;
            }

            bool result;
            if (bool.TryParse(bitMappedAttributeValue, out result))
            {
                return result;
            }

            return false;
        }

        private Conversion CreateBooleanConversion(XElement conversion, int bitIndex)
        {
            string[] values = conversion.Attribute("expression").Value.Split(',');
            if (values.Length != 2)
            {
                throw new InvalidDataException("Boolean expression must have two values separated by a comma");
            }

            return new Conversion(
                conversion.Attribute("units").Value,
                bitIndex,
                values[0],
                values[1]);
        }

        private Conversion CreateNumericConversion(XElement conversion)
        {
            return new Conversion(
                conversion.Attribute("units").Value,
                conversion.Attribute("expression").Value,
                conversion.Attribute("format").Value);
        }

        /// <summary>
        /// Load RAM parameters.
        /// </summary>
        private bool TryLoadRamParameters(out string errorMessage)
        {
            string pathToXml = Path.Combine(this.pathToXmlDirectory, "Parameters.RAM.xml");
            XDocument xml = XDocument.Load(pathToXml);
            List<Parameter> ramParameters = new List<Parameter>();
            foreach (XElement parameterElement in xml.Root.Elements("RamParameter"))
            {
                string parameterName = null;
                try
                {
                    parameterName = (string)parameterElement.Attribute("name").Value;

                    List<Conversion> conversions = GetConversions(parameterElement);

                    Dictionary<uint, uint> addresses = new Dictionary<uint, uint>();
                    foreach (XElement location in parameterElement.Elements("Location"))
                    {
                        string osidString = location.Attribute("os").Value;
                        uint osid = uint.Parse(osidString);

                        string addressString = location.Attribute("address").Value;
                        uint address = UnsignedHex.GetUnsignedHex(addressString);

                        addresses[osid] = address;
                    }

                    RamParameter parameter = new RamParameter(
                        parameterElement.Attribute("id").Value,
                        parameterName,
                        parameterElement.Attribute("description").Value,
                        parameterElement.Attribute("storageType").Value,
                        bool.Parse(parameterElement.Attribute("bitMapped").Value),
                        conversions,
                        addresses);

                    ramParameters.Add(parameter);
                    this.RamParameters.Add(parameter.Id, parameter);
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

        /// <summary>
        /// Load math parameters.
        /// </summary>
        private bool TryLoadMathParameters(out string errorMessage)
        {
            string pathToXml = Path.Combine(this.pathToXmlDirectory, "Parameters.Math.xml");
            XDocument xml = XDocument.Load(pathToXml);
            foreach (XElement parameterElement in xml.Root.Elements("MathParameter"))
            {
                string parameterName = null;
                try
                {
                    List<Conversion> conversions = GetConversions(parameterElement);

                    parameterName = (string)parameterElement.Attribute("name").Value;

                    string xId = parameterElement.Attribute("xParameterId").Value;
                    string xUnits = parameterElement.Attribute("xParameterConversion").Value;
                    Parameter xParameter = this.Parameters.Where(x => (x.Id == xId)).FirstOrDefault();
                    Conversion xConversion = xParameter.Conversions.Where(x => x.Units == xUnits).FirstOrDefault();

                    string yId = parameterElement.Attribute("yParameterId").Value;
                    string yUnits = parameterElement.Attribute("yParameterConversion").Value;
                    Parameter yParameter = this.Parameters.Where(y => (y.Id == yId)).FirstOrDefault();
                    Conversion yConversion = yParameter.Conversions.Where(y => y.Units == yUnits).FirstOrDefault();

                    MathParameter parameter = new MathParameter(
                        parameterElement.Attribute("id").Value,
                        parameterName,
                        parameterElement.Attribute("description").Value,
                        conversions,
                        new LogColumn(xParameter, xConversion),
                        new LogColumn(yParameter, yConversion));

                    parameters.Add(parameter);
                    this.MathParameters.Add(parameter.Id, parameter);
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
    }
}