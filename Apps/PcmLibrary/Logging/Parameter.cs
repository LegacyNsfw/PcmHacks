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

        public override string ToString()
        {
            return this.Units;
        }
    }

    /// <summary>
    /// Base class for various parameter types (PID, RAM, Math)
    /// </summary>
    public class Parameter
    {
        public string Id { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public IEnumerable<Conversion> Conversions { get; protected set; }

        public override string ToString()
        {
            return this.Name;
        }

        public bool TryGetConversion(string units, out Conversion conversion)
        {
            foreach(Conversion candidate in this.Conversions)
            {
                if (candidate.Units == units)
                {
                    conversion = candidate;
                    return true;
                }
            }

            conversion = null;
            return false;
        }
    }

    public class PcmParameter : Parameter
    {
        public int ByteCount { get; protected set; }
        public bool BitMapped { get; protected set; }
    }

    public class PidParameter : PcmParameter
    {
        public uint PID { get; private set; }

        /// <summary>
        /// Constructor for standard PID parameters.
        /// </summary>
        public PidParameter(
            uint id,
            string name,
            string description,
            int byteCount,
            bool bitMapped,
            IEnumerable<Conversion> conversions)
        {
            this.Id = id.ToString("X4");
            this.PID = UnsignedHex.GetUnsignedHex("0x" + this.Id);
            this.Name = name;
            this.Description = description;
            this.ByteCount = byteCount;
            this.BitMapped = bitMapped;
            this.Conversions = conversions;
        }
    }

    public class RamParameter : PcmParameter
    {
        private readonly Dictionary<uint, uint> addresses;

        /// <summary>
        /// Constructor for RAM parameters.
        /// </summary>
        public RamParameter(
            string id,
            string name,
            string description,
            int byteCount,
            bool bitMapped,
            IEnumerable<Conversion> conversions,
            Dictionary<uint, uint> addresses)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.ByteCount = byteCount;
            this.BitMapped = bitMapped;
            this.Conversions = conversions;
            this.addresses = addresses;
        }

        public bool TryGetAddress(uint osid, out uint address)
        {
            return this.addresses.TryGetValue(osid, out address);
        }
    }

    public class MathParameter : Parameter
    {
        public ProfileParameter XParameter { get; private set; }
        public ProfileParameter YParameter { get; private set; }

        public MathParameter(
            string id,
            string name,
            string description,
            IEnumerable<Conversion> conversions,
            ProfileParameter xParameter,
            ProfileParameter yParameter)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Conversions = conversions;

            this.XParameter = xParameter;
            this.YParameter = yParameter;
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
}
