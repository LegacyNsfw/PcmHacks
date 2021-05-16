using System;
using System.Collections.Generic;
using System.Text;

namespace PcmHacking
{
    /// <summary>
    /// Stores the math expression that converts a value from the PCM into 
    /// something humans can understand.
    /// </summary>
    public class Conversion
    {
        public string Units { get; private set; }

        public string Expression { get; private set; }

        public string Format { get; private set; }

        public bool IsBitMapped { get; private set; }

        public int BitIndex { get; private set; }

        public string TrueValue { get; private set; }

        public string FalseValue { get; private set; }

        public Conversion(string units, string expression, string format)
        {
            this.Units = units;
            this.Expression = Sanitize(expression);
            this.Format = format;
            this.IsBitMapped = false;
            this.BitIndex = -1;
            this.TrueValue = null;
            this.FalseValue = null;
        }

        public Conversion(string units, int bitIndex, string trueValue, string falseValue)
        {
            this.Units = units;
            this.Expression = "x";
            this.Format = "";
            this.IsBitMapped = true;
            this.BitIndex = bitIndex;
            this.TrueValue = trueValue;
            this.FalseValue = falseValue;
        }

        public override string ToString()
        {
            return this.Units;
        }

        /// <summary>
        /// The expression parser doesn't support bit-shift operators.
        /// So we hack them into division operators here.
        /// It's not pretty, but it's less ugly than changing the
        /// expressions in the XML file.
        /// </summary>
        private string Sanitize(string input)
        {
            int startIndex = input.IndexOf(">>");
            if (startIndex == -1)
            {
                return input;
            }

            int endIndex = startIndex;
            char shiftChar = ' ';
            for (int index = startIndex + 2; index < input.Length; index++)
            {
                endIndex = index;
                shiftChar = input[index];
                if (shiftChar == ' ')
                {
                    continue;
                }
                else
                {
                    endIndex++;
                    break;
                }
            }

            int shiftCount = shiftChar - '0';
            if (shiftCount < 0 || shiftCount > 15)
            {
                throw new InvalidOperationException(
                    string.Format("Unable to parse >> operator in \"{0}\"", input));
            }

            string oldText = input.Substring(startIndex, endIndex - startIndex);
            string newText = string.Format("/{0}", Math.Pow(2, shiftCount));
            return input.Replace(oldText, newText);
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

    /// <summary>
    /// Base class for parameters that come directly from the PCM - as opposed 
    /// to Math parameters, which are only indirectly from the PCM.
    /// </summary>
    public abstract class PcmParameter : Parameter
    {
        public string StorageType { get; protected set; }

        public int ByteCount
        {
            get
            {
                switch(this.StorageType)
                {
                    case "uint8":
                    case "int8":
                        return 1;

                    case "uint16":
                    case "int16":
                        return 2;

                    default:
                        return 0;
                }
            }
        }

        public bool IsSigned
        {
            get
            {
                switch (this.StorageType)
                {
                    case "int8":
                    case "int16":
                        return true;

                    case "uint8":
                    case "uint16":
                        return false;

                    default:
                        return false;
                }
            }
        }

        public bool BitMapped { get; protected set; }
    }

    /// <summary>
    /// These parameters have a PID number that is the same for all operating
    /// systems. (Though not not all operating systems support all PIDs.)
    /// </summary>
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
            string storageType,
            bool bitMapped,
            IEnumerable<Conversion> conversions)
        {
            this.Id = id.ToString("X4");
            this.PID = UnsignedHex.GetUnsignedHex("0x" + this.Id);
            this.Name = name;
            this.Description = description;
            this.StorageType = storageType;
            this.BitMapped = bitMapped;
            this.Conversions = conversions;
        }

        public override bool IsSupported(uint osid)
        {
            return true;
        }
    }

    /// <summary>
    /// These parameters are read from RAM in the PCM, and the RAM addresses
    /// are unique to each operating system.
    /// </summary>
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
            string storageType,
            bool bitMapped,
            IEnumerable<Conversion> conversions,
            Dictionary<uint, uint> addresses)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.StorageType = storageType;
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

    /// <summary>
    /// These parameters are computed from other parameters.
    /// </summary>
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
