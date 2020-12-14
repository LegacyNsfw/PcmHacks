using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PcmHacking
{
    public static class UnsignedHex
    {
        public static string GetUnsignedHex(UInt32 value)
        {
            return "0x" + value.ToString("X");
        }

        public static UInt32 GetUnsignedHex(string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue))
            {
                return 0;
            }

            if (!rawValue.StartsWith("0x"))
                throw new XmlSchemaException("Unexpected format of unsigned hex value: " + rawValue);

            uint result;
            if (uint.TryParse(
                rawValue.Substring(2),
                NumberStyles.HexNumber,
                CultureInfo.CurrentCulture,
                out result))
            {
                return result;
            }

            throw new JsonSerializationException("Unable to parse hex value: " + rawValue);
        }
    }

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

    public class UnsignedHexValueConverter : JsonConverter<UnsignedHexValue>
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
}
