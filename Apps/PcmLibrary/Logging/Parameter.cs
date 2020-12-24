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
    public abstract class Parameter
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

        public abstract bool IsSupported(uint osid);
    }

    public abstract class PcmParameter : Parameter
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

        public override bool IsSupported(uint osid)
        {
            return true;
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

        public override bool IsSupported(uint osid)
        {
            uint address;
            return this.TryGetAddress(osid, out address);
        }
    }

    public class MathParameter : Parameter
    {
        public LogColumn XColumn { get; private set; }
        public LogColumn YColumn { get; private set; }

        public MathParameter(
            string id,
            string name,
            string description,
            IEnumerable<Conversion> conversions,
            LogColumn xColumn,
            LogColumn yColumn)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Conversions = conversions;

            this.XColumn = xColumn;
            this.YColumn = yColumn;
        }

        public override bool IsSupported(uint osid)
        {
            return this.XColumn.Parameter.IsSupported(osid) && this.YColumn.Parameter.IsSupported(osid);
        }
    }

}
