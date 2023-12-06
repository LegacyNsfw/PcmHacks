using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace PcmHacking
{
    /// <summary>
    /// This loads the collection of known parameters from XML files.
    /// </summary>
    public class ParameterDatabase
    {
        private string pathToXmlDirectory;

        private List<Parameter> parameters = new List<Parameter>();
        private Dictionary<UInt32, IEnumerable<CanParameter>> canParameters = new Dictionary<UInt32, IEnumerable<CanParameter>>();

        /// <summary>
        /// Constructor
        /// </summary>
        public ParameterDatabase(string pathToXmlDirectory)
        {
            this.pathToXmlDirectory = pathToXmlDirectory;
        }

        /// <summary>
        ///Gets a parameter using the specified generic type, with the specified id. Will throw if a parameter is not found.
        /// </summary>
        /// <typeparam name="T">The type of parameter, must be subclass of Parameter</typeparam>
        /// <param name="id">the string ID of the parameter to look for</param>
        /// <returns>The parameter, if found.</returns>
        public T GetParameter<T>(string id) where T : Parameter
        {
            try
            {
                return this.parameters.First(p => p is T && p.Id == id) as T;
            }
            catch(InvalidOperationException)
            {
                uint pid;
                if (typeof(T) == typeof(PidParameter) &&
                    uint.TryParse(id, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out pid))
                {
                    var result = this.parameters.FirstOrDefault(p => (p as PidParameter)?.PID == pid);
                    if (result != null)
                    {
                        return result as T;
                    }                    
                }

                throw new Exception($"Unable to find matching parameter for ${typeof(T).Name} '{id}'");
            }
        }

        /// <summary>
        /// returns a list of parameters that support the specified os
        /// </summary>
        /// <param name="osId">the os to search for</param>
        /// <returns>an ienumerable of parameters</returns>
        public IEnumerable<Parameter> ListParametersBySupportedOs(uint osId)
        {
            return this.parameters.Where(p => p.IsSupported(osId));
        }

        /// <summary>
        /// adds a parameter to the database, will not allow parameters with duplicate IDs, throws exception in that case.
        /// </summary>
        /// <param name="parameter">the parameter to add</param>
        public void AddParameter(Parameter parameter)
        {
            if (this.parameters.Any(P => P.Id == parameter.Id))
            {
                throw new Exception(String.Format("Duplicate parameter ID: {0}", parameter.Id));
            }

            this.parameters.Add(parameter);
        }

        /// <summary>
        /// Load everything.
        /// </summary>
        public void LoadDatabase()
        {
            this.LoadStandardParameters();
            this.LoadRamParameters();
            this.LoadMathParameters();
            this.LoadCanParameters();
        }

        /// <summary>
        /// Load standard parameters.
        /// </summary>
        private void LoadStandardParameters()
        {
            string pathToXml = Path.Combine(this.pathToXmlDirectory, "Parameters.Standard.xml");
            XDocument xml = XDocument.Load(pathToXml);

            foreach (XElement parameterElement in xml.Root.Elements("Parameter"))
            {
                var osElements = parameterElement.Elements("OS");
                List<uint> osids = new List<uint>();

                foreach (XElement os in osElements)
                {
                    string osidString = os.Attribute("id").Value;

                    if (osidString.ToLower() == "all")
                    {
                        osids.Clear(); //going to use no osids as all supported.
                        break;
                    }

                    uint osid = uint.Parse(osidString);
                        
                    osids.Add(osid);
                }

                List<Conversion> conversions = GetConversions(parameterElement);

                PidParameter parameter = new PidParameter(
                    parameterElement.Attribute("id").Value,
                    parameterElement.Attribute("name").Value,
                    parameterElement.Attribute("description").Value,
                    parameterElement.Attribute("storageType").Value,
                    bool.Parse(parameterElement.Attribute("bitMapped").Value),
                    conversions,
                    UnsignedHex.GetUnsignedHex("0x" + parameterElement.Attribute("pid").Value),
                    osids);

                AddParameter(parameter);
            }
        }

        /// <summary>
        /// Load RAM parameters.
        /// </summary>
        private void LoadRamParameters()
        {
            string pathToXml = Path.Combine(this.pathToXmlDirectory, "Parameters.RAM.xml");
            XDocument xml = XDocument.Load(pathToXml);

            foreach (XElement parameterElement in xml.Root.Elements("RamParameter"))
            {
                Dictionary<uint, uint> addresses = new Dictionary<uint, uint>();
                foreach (XElement location in parameterElement.Elements("Location"))
                {
                    string osidString = location.Attribute("os").Value;
                    uint osid = uint.Parse(osidString);

                    string addressString = location.Attribute("address").Value;
                    uint address = UnsignedHex.GetUnsignedHex(addressString);

                    addresses[osid] = address;
                }

                List<Conversion> conversions = GetConversions(parameterElement);

                RamParameter parameter = new RamParameter(
                    parameterElement.Attribute("id").Value,
                    parameterElement.Attribute("name").Value,
                    parameterElement.Attribute("description").Value,
                    parameterElement.Attribute("storageType").Value,
                    bool.Parse(parameterElement.Attribute("bitMapped").Value),
                    conversions,
                    addresses);

                AddParameter(parameter);
            }
        }

        /// <summary>
        /// Load math parameters.
        /// </summary>
        private void LoadMathParameters()
        {
            string pathToXml = Path.Combine(this.pathToXmlDirectory, "Parameters.Math.xml");
            XDocument xml = XDocument.Load(pathToXml);
            foreach (XElement parameterElement in xml.Root.Elements("MathParameter"))
            {
                string parameterName = parameterElement.Attribute("name").Value;

                List<Conversion> conversions = GetConversions(parameterElement);

                string xId = parameterElement.Attribute("xParameterId").Value;
                string xUnits = parameterElement.Attribute("xParameterConversion").Value;
                string yId = parameterElement.Attribute("yParameterId").Value;
                string yUnits = parameterElement.Attribute("yParameterConversion").Value;

                LogColumn xLogColumn = BuildLogColumnForMathParameter(xId, xUnits, parameterName);
                LogColumn yLogColumn = BuildLogColumnForMathParameter(yId, yUnits, parameterName);

                MathParameter parameter = new MathParameter(
                    parameterElement.Attribute("id").Value,
                    parameterName,
                    parameterElement.Attribute("description").Value,
                    conversions,
                    xLogColumn,
                    yLogColumn);

                AddParameter(parameter);
            }
        }

        private void LoadCanParameters()
        {
            string pathToXml = Path.Combine(this.pathToXmlDirectory, "Parameters.CAN.xml");
            XDocument xml = XDocument.Load (pathToXml);
            foreach (XElement messageElement in xml.Root.Elements("Message"))
            {
                string messageIdString = messageElement.Attribute("id").Value;
                UInt32 messageId = UInt32.Parse(messageIdString, NumberStyles.HexNumber);

                List<CanParameter> parameters = new List<CanParameter>();

                foreach (XElement parameterElement in messageElement.Elements("Parameter"))
                {
                    string id = parameterElement.Attribute("id").Value;
                    string name = parameterElement.Attribute("name").Value;
                    string description = parameterElement.Attribute("description").Value;

                    uint firstByte = uint.Parse(parameterElement.Attribute("firstByte").Value);
                    uint byteCount = uint.Parse(parameterElement.Attribute("byteCount").Value);
                    bool highByteFirst = bool.Parse(parameterElement.Attribute("highByteFirst").Value);

                    List<Conversion> conversions = GetConversions(parameterElement);

                    CanParameter parameter = new CanParameter(messageId, firstByte, byteCount, highByteFirst, id, name, description, conversions);

                    parameters.Add(parameter);
                }

                this.canParameters[messageId] = parameters;
            }
        }

        private LogColumn BuildLogColumnForMathParameter(string id, string units, string parameterName)
        {
            Parameter xParameter = this.parameters.Where(x => (x.Id == id)).FirstOrDefault();

            if (xParameter == null)
            {
                throw new Exception(String.Format("No parameter found for {0} in {1}", id, parameterName));
            }

            Conversion xConversion = xParameter.Conversions.Where(x => x.Units == units).FirstOrDefault();

            if (xConversion == null)
            {
                throw new Exception(String.Format("No conversion found for {0} in {1}", units, parameterName));
            }

            return new LogColumn(xParameter, xConversion);
        }

        List<Conversion> GetConversions(XElement parameterElement)
        {
            List<Conversion> returnConversions = new List<Conversion>();

            foreach (XElement conversionXml in parameterElement.Elements("Conversion"))
            {
                if (IsBitmapped(parameterElement))
                {
                    int bitIndex = Convert.ToInt32(parameterElement.Attribute("bitIndex")?.Value);
                    returnConversions.Add(CreateBooleanConversion(conversionXml, bitIndex));
                }
                else
                {
                    returnConversions.Add(CreateNumericConversion(conversionXml));
                }
            }

            return returnConversions;
        }

        private bool IsBitmapped(XElement parameterElement)
        {
            string bitMappedAttributeValue = parameterElement.Attribute("bitMapped")?.Value;
            if (string.IsNullOrEmpty(bitMappedAttributeValue))
            {
                return false;
            }

            if (bool.TryParse(bitMappedAttributeValue, out bool result))
            {
                return result;
            }

            return false;
        }

        private Conversion CreateBooleanConversion(XElement conversionXml, int bitIndex)
        {
            string[] values = conversionXml.Attribute("expression").Value.Split(',');

            if (values.Length != 2)
            {
                throw new Exception("Boolean expression must have two values separated by a comma");
            }

            return new Conversion(
                conversionXml.Attribute("units").Value,
                bitIndex,
                values[0],
                values[1]);
        }

        private Conversion CreateNumericConversion(XElement conversionXml)
        {
            return new Conversion(
                conversionXml.Attribute("units").Value,
                conversionXml.Attribute("expression").Value,
                conversionXml.Attribute("format").Value);
        }
    }
}