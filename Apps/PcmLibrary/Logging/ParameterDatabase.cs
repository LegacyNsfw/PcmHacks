using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace PcmHacking
{
    public class Conversion
    {
        public string Units { get; private set; }
        public string Expression { get; private set; }
        public string Format { get; private set; }
        public Conversion(string units, string expression, string format)
        {
            this.Units = units;
            this.Expression = expression;
            this.Format = format;
        }
    }

    public enum ParameterType
    {
        Invalid,
        PID,
        RAM
    };

    public class Parameter
    {
        public string Id { get; private set; }
        public uint Address { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public ParameterType Type { get; private set; }
        public int ByteCount { get; private set; }
        public bool BitMapped { get; private set; }
        public IEnumerable<Conversion> Conversions { get; private set; }

        /// <summary>
        /// Constructor for standard PID parameters.
        /// </summary>
        public Parameter(
            uint id,
            string name,
            string description,
            int byteCount,
            bool bitMapped,
            IEnumerable<Conversion> conversions)
        {
            this.Id = id.ToString("X4");
            this.Address = UnsignedHex.GetUnsignedHex("0x" + this.Id);
            this.Name = name;
            this.Description = description;
            this.Type = ParameterType.PID;
            this.ByteCount = byteCount;
            this.BitMapped = bitMapped;
            this.Conversions = conversions;
        }

        /// <summary>
        /// Constructor for RAM parameters.
        /// </summary>
        public Parameter(
            string id,
            uint address,
            string name,
            string description,
            int byteCount,
            bool bitMapped,
            IEnumerable<Conversion> conversions)
        {
            this.Id = id;
            this.Address = address;
            this.Name = name;
            this.Description = description;
            this.Type = ParameterType.PID;
            this.ByteCount = byteCount;
            this.BitMapped = bitMapped;
            this.Conversions = conversions;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public class ProfileParameter 
    {
        public Parameter Parameter { get; private set; }
        public Conversion Conversion { get; private set; }

        public ProfileParameter(Parameter parameter, Conversion conversion)
        {
            this.Parameter = parameter;
            this.Conversion = conversion;
        }

        public override string ToString()
        {
            return base.ToString() + ", " + this.Conversion.ToString();
        }
    }


    public class ParameterDatabase
    {
        public static IEnumerable<Parameter> Parameters { get; private set; }

        public static bool TryLoad(string pathToXml, out string errorMessage)
        {
            XDocument xml = XDocument.Load(pathToXml);
            List<Parameter> parameters = new List<Parameter>();
            foreach (XElement parameter in xml.Root.Elements("Parameter"))
            {
                string parameterName = null;
                try
                {
                    List<Conversion> conversions = new List<Conversion>();
                    foreach (XElement conversion in parameter.Elements("Conversion"))
                    {
                        conversions.Add(
                            new Conversion(
                                conversion.Attribute("units").Value,
                                conversion.Attribute("expression").Value,
                                conversion.Attribute("format").Value));
                    }

                    string parameterType = (string)parameter.Attribute("type");
                    parameterName = (string)parameter.Attribute("name").Value;
                    if (parameterType == "PID")
                    {
                        parameters.Add(
                            new Parameter(
                                UnsignedHex.GetUnsignedHex("0x" + parameter.Attribute("id").Value),
                                parameterName,
                                parameter.Attribute("description").Value,
                                int.Parse(parameter.Attribute("byteCount").Value),
                                bool.Parse(parameter.Attribute("bitMapped").Value),
                                conversions));
                    }
                    else if (parameterType == "RAM")
                    {
                        parameters.Add(
                            new Parameter(
                                parameter.Attribute("id").Value,
                                UnsignedHex.GetUnsignedHex("0x" + parameter.Attribute("address").Value),
                                parameterName,
                                parameter.Attribute("description").Value,
                                int.Parse(parameter.Attribute("byteCount").Value),
                                bool.Parse(parameter.Attribute("bitMapped").Value),
                                conversions));
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
            Parameters = parameters;
            return true;
        }
    }
}
