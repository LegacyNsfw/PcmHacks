﻿using System;
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

        private List<Parameter> parameters;

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

            foreach (XElement parameterElement in xml.Root.Elements("Parameter"))
            {
                string parameterName = null;
                try
                {
                    var osElements = parameterElement.Elements("OS");
                    List<uint> osids = new List<uint>();

                    foreach (XElement os in osElements)
                    {
                        string osidString = os.Attribute("id").Value;

                        if (osidString.ToLower() == "all")
                        {
                            osids.Clear(); //going to use no osids as all supported.
                        }

                        uint osid = uint.Parse(osidString);
                        
                        osids.Add(osid);
                    }

                    List<Conversion> conversions = GetConversions(parameterElement);

                    parameterName = (string)parameterElement.Attribute("name").Value;

                    PidParameter parameter = new PidParameter(
                        parameterElement.Attribute("id").Value,
                        parameterName,
                        parameterElement.Attribute("description").Value,
                        parameterElement.Attribute("storageType").Value,
                        bool.Parse(parameterElement.Attribute("bitMapped").Value),
                        conversions,
                        UnsignedHex.GetUnsignedHex("0x" + parameterElement.Attribute("pid").Value),
                        osids);

                    parameters.Add(parameter);
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

                    parameters.Add(parameter);
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
                    Parameter xParameter = this.parameters.Where(x => (x.Id == xId)).FirstOrDefault();
                    Conversion xConversion = xParameter.Conversions.Where(x => x.Units == xUnits).FirstOrDefault();

                    string yId = parameterElement.Attribute("yParameterId").Value;
                    string yUnits = parameterElement.Attribute("yParameterConversion").Value;
                    Parameter yParameter = this.parameters.Where(y => (y.Id == yId)).FirstOrDefault();
                    Conversion yConversion = yParameter.Conversions.Where(y => y.Units == yUnits).FirstOrDefault();

                    MathParameter parameter = new MathParameter(
                        parameterElement.Attribute("id").Value,
                        parameterName,
                        parameterElement.Attribute("description").Value,
                        conversions,
                        new LogColumn(xParameter, xConversion),
                        new LogColumn(yParameter, yConversion));

                    parameters.Add(parameter);
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