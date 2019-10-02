using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PcmHacking
{
    public struct UnsignedHexValue
    {
        private uint value;
        public UnsignedHexValue(uint value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return $"0x{this.value:X}";
        }

        public static implicit operator uint(UnsignedHexValue hex) { return hex.value; }
        public static implicit operator UnsignedHexValue(uint x) { return new UnsignedHexValue(x); }
    }

    public class UnsignedHexValueConverter :JsonConverter<UnsignedHexValue>
    {
        public override UnsignedHexValue ReadJson(JsonReader reader, Type objectType, UnsignedHexValue existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            if (string.IsNullOrEmpty(s))
            {
                return 0;
            }

            if (!s.StartsWith("0x"))
                throw new JsonSerializationException();

            uint result;
            if (uint.TryParse(
                s.Substring(2),
                NumberStyles.HexNumber,
                CultureInfo.CurrentCulture,
                out result))
            {
                return result;
            }

            throw new JsonSerializationException("Unable to parse hex value: " + s);
        }

        public override void WriteJson(JsonWriter writer, UnsignedHexValue value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
    
    //[DataContract]
    public class Conversion
    {
        public string Name { get; set; }
        public string Expression { get; set; }
        public string Format { get; set; }
        public override string ToString()
        {
            return Name + " (" + Expression + ")";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    //[DataContract]
    public class Parameter
    {
        public string Name { get; set; }

        public DefineBy DefineBy { get; set; }

        public int ByteCount { get; set; }

        public UnsignedHexValue Address { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2} bytes at {3}", this.Name, this.DefineBy, this.ByteCount, this.Address);
        }
    }

    //[DataContract]
    public class DatabaseParameter : Parameter
    { 
        public List<Conversion> Conversions { get; set; }
    }

    //[DataContract]
    public class ProfileParameter : Parameter
    {
        public Conversion Conversion { get; set; }

        public override string ToString()
        {
            return base.ToString() + ", " + this.Conversion.ToString();
        }
    }

    //[DataContract]
    public class ParameterGroup
    {
        public const int MaxBytes = 6;

        public UnsignedHexValue Dpid { get; set; }

        public IList<ProfileParameter> Parameters { get; set; }

        public ParameterGroup()
        {
            this.Parameters = new List<ProfileParameter>();
        }

        public int TotalBytes
        {
            get
            {
                return Parameters.Sum(x => x.ByteCount);
            }
        }

        public override string ToString()
        {
            return string.Join(", ", new string[] { this.Dpid.ToString() }.Concat(this.Parameters.Select(x => x.Name)));
        }

        public bool TryAddParameter(ProfileParameter parameter)
        {
            if (this.TotalBytes + parameter.ByteCount > MaxBytes)
            {
                return false;
            }

            this.Parameters.Add(parameter);
            return true;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    //[DataContract]
    public class LogProfile
    {
        public const int MaxGroups = 3;

        public IList<ParameterGroup> ParameterGroups { get; set; }

        public int ParameterCount
        {
            get
            {
                return this.ParameterGroups.Sum(x => x.Parameters.Count);
            }
        }

        public LogProfile()
        {
            this.ParameterGroups = new List<ParameterGroup>();
        }

        public bool TryAddGroup(ParameterGroup group)
        {
            if (this.ParameterGroups.Count + 1 > MaxGroups)
            {
                return false;
            }

            this.ParameterGroups.Add(group);
            return true;
        }

        public string GetParameterNames(string separator)
        {
            return string.Join(
                separator, 
                this.ParameterGroups.SelectMany(
                    group => group.Parameters.Select(
                        parameter => parameter.Name)));
        }
    }
}
