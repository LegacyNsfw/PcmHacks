using System;
using System.Collections.Generic;
using System.Text;

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
            string id,
            string name,
            string description,
            int byteCount,
            bool bitMapped,
            IEnumerable<Conversion> conversions)
        {
            this.Id = id;
            this.Address = UnsignedHex.GetUnsignedHex(this.Id);
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
        public IEnumerable<Parameter> Parameters { get; private set; }
        public ParameterDatabase(IEnumerable<Parameter> parameters)
        {
            this.Parameters = parameters;
        }
    }
}
