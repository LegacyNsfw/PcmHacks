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
        private string pathToXmlDirectory;

        private List<Parameter> parameters = new List<Parameter>();

        /// <summary>
        /// Constructor
        /// </summary>
        public ParameterDatabase(string pathToXmlDirectory)
        {
            this.pathToXmlDirectory = pathToXmlDirectory;
        }

        /// <summary>
        /// tries to get a parameter using the specified generic type, with the specified id. Returns true if one is found, false if not.
        /// </summary>
        /// <typeparam name="T">The type of parameter, must be subclass of Parameter</typeparam>
        /// <param name="id">the string ID of the parameter to look for</param>
        /// <param name="parameter">out param to return the parameter</param>
        /// <returns>boolean representing the found state of the parameter that was being searched for.</returns>
        public bool TryGetParameter<T>(string id, out T parameter) where T : Parameter
        {
            parameter = this.parameters.FirstOrDefault(p => p is T && p.Id == id) as T;

            return parameter != null;
        }/// <summary>
         /// 
         /// </summary>
         /// <param name="parameter">the parameter to add</param>
         /// <exception cref="Exception">if the parameter to add has the same id as another parameter already in the database, this exception will fire.</exception>


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
        /// tries to add a parameter to the database, will not allow parameters with duplicate IDs
        /// </summary>
        /// <param name="parameter">the parameter to add</param>
        /// <param name="errorMessage">if the parameter to add has the same id as another parameter already in the database, this message will be populated</param>
        /// <returns>false if it could not add the parameter, true if the parameter was added successfully</returns>
        public bool TryAddParameter(Parameter parameter, out string errorMessage)
        {
            if (this.parameters.Any(P => P.Id == parameter.Id))
            {
                errorMessage = string.Format("Duplicate parameter ID:{0}", parameter.Id);
                return false;
            }

            this.parameters.Add(parameter);
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Load everything.
        /// </summary>
        public bool TryLoad(out string errorMessage)
        {
            if (!this.TryLoadStandardParameters(out errorMessage) ||
                !this.TryLoadRamParameters(out errorMessage) ||
                !this.TryLoadMathParameters(out errorMessage))
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

                if (!TryGetConversions(parameterElement, out List<Conversion> conversions, out errorMessage))
                {
                    return false;
                }

                PidParameter parameter = new PidParameter(
                    parameterElement.Attribute("id").Value,
                    parameterElement.Attribute("name").Value,
                    parameterElement.Attribute("description").Value,
                    parameterElement.Attribute("storageType").Value,
                    bool.Parse(parameterElement.Attribute("bitMapped").Value),
                    conversions,
                    UnsignedHex.GetUnsignedHex("0x" + parameterElement.Attribute("pid").Value),
                    osids);

                if (!TryAddParameter(parameter, out errorMessage))
                {
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Load RAM parameters.
        /// </summary>
        private bool TryLoadRamParameters(out string errorMessage)
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

                if (!TryGetConversions(parameterElement, out List<Conversion> conversions, out errorMessage))
                {
                    return false;
                }

                RamParameter parameter = new RamParameter(
                    parameterElement.Attribute("id").Value,
                    parameterElement.Attribute("name").Value,
                    parameterElement.Attribute("description").Value,
                    parameterElement.Attribute("storageType").Value,
                    bool.Parse(parameterElement.Attribute("bitMapped").Value),
                    conversions,
                    addresses);

                if (!TryAddParameter(parameter, out errorMessage))
                {
                    return false;
                }
            }

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
                string parameterName = parameterElement.Attribute("name").Value;

                if (!TryGetConversions(parameterElement, out List<Conversion> conversions, out errorMessage))
                {
                    return false;
                }

                string xId = parameterElement.Attribute("xParameterId").Value;
                string xUnits = parameterElement.Attribute("xParameterConversion").Value;
                string yId = parameterElement.Attribute("yParameterId").Value;
                string yUnits = parameterElement.Attribute("yParameterConversion").Value;

                string logColumnErrorMessage = null;
                if (!TryBuildLogColumnForMathParameter(xId, xUnits, parameterName, out LogColumn xLogColumn, out errorMessage) ||
                    !TryBuildLogColumnForMathParameter(yId, yUnits, parameterName, out LogColumn yLogColumn, out errorMessage))
                {
                    return false;
                }

                MathParameter parameter = new MathParameter(
                    parameterElement.Attribute("id").Value,
                    parameterName,
                    parameterElement.Attribute("description").Value,
                    conversions,
                    xLogColumn,
                    yLogColumn);

                if (!TryAddParameter(parameter, out errorMessage))
                {
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }

        private bool TryBuildLogColumnForMathParameter(string id, string units, string parameterName, out LogColumn logColumn, out string errorMessage)
        {
            Parameter xParameter = this.parameters.Where(x => (x.Id == id)).FirstOrDefault();

            if (xParameter == null)
            {
                logColumn = null;
                errorMessage = String.Format("No parameter found for {0} in {1}", id, parameterName);
                return false;
            }

            Conversion xConversion = xParameter.Conversions.Where(x => x.Units == units).FirstOrDefault();

            if (xConversion == null)
            {
                logColumn = null;
                errorMessage = String.Format("No conversion found for {0} in {1}", units, parameterName);
                return false;
            }

            logColumn = new LogColumn(xParameter, xConversion);
            errorMessage = null;
            return true;
        }

        bool TryGetConversions(XElement parameterElement, out List<Conversion> conversions, out string errorMessage)
        {
            List<Conversion> returnConversions = new List<Conversion>();

            foreach (XElement conversionXml in parameterElement.Elements("Conversion"))
            {
                int bitIndex = Convert.ToInt32(parameterElement.Attribute("bitIndex")?.Value);
                Conversion conversion = null;

                if (IsBitmapped(parameterElement) && TryCreateBooleanConversion(conversionXml, bitIndex, out conversion, out errorMessage) ||
                    TryCreateNumericConversion(conversionXml, out conversion, out errorMessage))
                {
                    returnConversions.Add(conversion);
                }
                else
                {
                    conversions = null;
                    return false;
                }
            }

            conversions = returnConversions;
            errorMessage = null;
            return true;
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

        private bool TryCreateBooleanConversion(XElement conversionXml, int bitIndex, out Conversion conversion, out string errorMessage)
        {
            string[] values = conversionXml.Attribute("expression").Value.Split(',');
            if (values.Length != 2)
            {
                conversion = null;
                errorMessage = "Boolean expression must have two values separated by a comma";
                return false;
            }

            conversion = new Conversion(
                conversionXml.Attribute("units").Value,
                bitIndex,
                values[0],
                values[1]);
            errorMessage = null;
            return true;
        }

        private bool TryCreateNumericConversion(XElement conversionXml, out Conversion conversion, out string errorMessage)
        {
            errorMessage = null;
            conversion = new Conversion(
                conversionXml.Attribute("units").Value,
                conversionXml.Attribute("expression").Value,
                conversionXml.Attribute("format").Value);

            return true; // can probably add validation here now.
        }
    }
}