using DynamicExpresso;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PcmHacking
{
    public class MathValueAndDependencies
    {
        public MathValue MathValue { get; private set; }
        public ProfileParameter XParameter { get; private set; }
        public Conversion XConversion { get; private set; }
        public ProfileParameter YParameter { get; private set; }
        public Conversion YConversion { get; private set; }

        public MathValueAndDependencies(
            MathValue mathValue,
            ProfileParameter xParameter,
            Conversion xConversion,
            ProfileParameter yParameter,
            Conversion yConversion)
        {
            this.MathValue = mathValue;
            this.XParameter = xParameter;
            this.XConversion = xConversion;
            this.YParameter = yParameter;
            this.YConversion = yConversion;
        }
    }

    public class MathValueProcessor
    {
        private readonly DpidConfiguration profile;
        private List<MathValueAndDependencies> mathValues;

        public MathValueProcessor(DpidConfiguration profile, MathValueConfiguration mathValueConfiguration)
        {
            this.profile = profile;
            this.mathValues = new List<MathValueAndDependencies>();

            foreach (MathValue mathValue in mathValueConfiguration.MathValues)
            {
                ProfileParameter xParameter = null;
                Conversion xConversion = null;
                ProfileParameter yParameter = null;
                Conversion yConversion = null;

                foreach (ProfileParameter parameter in this.profile.AllParameters)
                {
                    // TODO: Find the parameter in a configuration file that contains all parameters and conversions,
                    // pick the appropriate conversion even if it's not what the user chose for this log profile.
                    if (parameter.Parameter.Name == mathValue.XParameter)
                    {
                        xParameter = parameter;
                        xConversion = parameter.Conversion;
                    }

                    if (parameter.Parameter.Name == mathValue.YParameter)
                    {
                        yParameter = parameter;
                        yConversion = parameter.Conversion;
                    }
                }

                if ((xParameter != null) &&
                    (xConversion != null) &&
                    (yParameter != null) &&
                    (yConversion != null))
                {
                    MathValueAndDependencies valueAndDependencies = new MathValueAndDependencies(
                        mathValue, 
                        xParameter, 
                        xConversion, 
                        yParameter, 
                        yConversion);

                    this.mathValues.Add(valueAndDependencies);
                }
            }
        }

        public IEnumerable<string> GetHeaders()
        {
            return this.mathValues.Select(x => x.MathValue.Name);
        }

        public IEnumerable<MathValue> GetMathValues()
        {
            return this.mathValues.Select(x => x.MathValue);
        }

        public IEnumerable<string> GetMathValues(DpidValues dpidValues)
        {
            List<string> result = new List<string>();
            foreach(MathValueAndDependencies value in this.mathValues)
            {
                double xParameterValue = dpidValues[value.XParameter].RawValue;
                Interpreter xConverter = new Interpreter();
                xConverter.SetVariable("x", xParameterValue);
                double xConverted = xConverter.Eval<double>(value.XConversion.Expression);

                double yParameterValue = dpidValues[value.YParameter].RawValue;
                Interpreter yConverter = new Interpreter();
                xConverter.SetVariable("x", yParameterValue);
                double YConverted = xConverter.Eval<double>(value.YConversion.Expression);

                Interpreter finalConverter = new Interpreter();
                finalConverter.SetVariable("x", xConverted);
                finalConverter.SetVariable("y", YConverted);
                double converted = finalConverter.Eval<double>(value.MathValue.Formula);
                result.Add(converted.ToString(value.MathValue.Format));
            }

            return result;
        }
    }
}
