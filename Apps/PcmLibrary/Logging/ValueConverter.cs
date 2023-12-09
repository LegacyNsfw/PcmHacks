using System;
using System.Collections.Generic;
using System.Text;
using DynamicExpresso;

namespace PcmHacking
{
    class ValueConverter
    {
        public static string Convert(double value, string name, Conversion conversion)
        {
            double convertedValue;
            string formattedValue;

            if (conversion.IsBitMapped)
            {
                int bits = (int)value;
                bits = bits >> conversion.BitIndex;
                bool flag = (bits & 1) != 0;

                convertedValue = value;
                formattedValue = flag ? conversion.TrueValue : conversion.FalseValue;
            }
            else
            {
                try
                {
                    Interpreter interpreter = new Interpreter();
                    interpreter.SetVariable("x", value);

                    convertedValue = interpreter.Eval<double>(conversion.Expression);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(
                        string.Format("Unable to evaluate expression \"{0}\" for parameter \"{1}\"",
                            conversion.Expression,
                            name),
                        exception);
                }

                string format = conversion.Format;
                if (string.IsNullOrWhiteSpace(format))
                {
                    format = "0.00";
                }

                formattedValue = convertedValue.ToString(format);
            }

            return formattedValue;

        }
    }
}
