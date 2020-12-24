using System;
using System.Globalization;

namespace PcmHacking
{
    /// <summary>
    /// Utilities for converting uints to and from hexadecimal notation.
    /// </summary>
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

//            if (!rawValue.StartsWith("0x"))
//                throw new ArgumentException("Unexpected format of unsigned hex value: " + rawValue);

            uint result;
            if (uint.TryParse(
                rawValue.Substring(2),
                NumberStyles.HexNumber,
                CultureInfo.CurrentCulture,
                out result))
            {
                return result;
            }

            throw new ArgumentException("Unable to parse hex value: " + rawValue);
        }
    }
}
