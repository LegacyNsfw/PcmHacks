using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PcmHacking;

namespace Tests
{
    [TestClass]
    public class MathTests
    {
        [TestMethod]
        public void MathValueTest()
        {
            ProfileParameter rpm = new ProfileParameter();
            rpm.Name = "Engine Speed";
            rpm.Conversion = new Conversion();
            rpm.Conversion.Name = "RPM";
            rpm.Conversion.Expression = "x";

            ProfileParameter maf = new ProfileParameter();
            maf.Name = "Mass Air Flow";
            maf.Conversion = new Conversion();
            maf.Conversion.Name = "g/s";
            maf.Conversion.Expression = "x";

            MathValue load = new MathValue();
            load.XParameter = rpm.Name;
            load.XConversion = rpm.Conversion.Name;
            load.YParameter = maf.Name;
            load.YConversion = maf.Conversion.Name;
            load.Format = "0.00";
            load.Formula = "(y*60)/x";

            LogProfile profile = new LogProfile();
            profile.ParameterGroups.Add(new ParameterGroup());
            profile.ParameterGroups[0].Parameters.Add(rpm);
            profile.ParameterGroups[0].Parameters.Add(maf);

            //MockDevice mockDevice = new MockDevice();
            //MockLogger mockLogger = new MockLogger();
            //Vehicle vehicle = new Vehicle(
            //    new MockDevice(),
            //    new Protocol(),
            //    mockLogger,
            //    new ToolPresentNotifier(mockDevice, mockLogger));
            //Logger logger = new Logger(vehicle, profile, mathValueConfiguration);

            //MathValueConfigurationLoader loader = new MathValueConfigurationLoader();
            //loader.Initialize();
            MathValueConfiguration mathValueConfiguration = new MathValueConfiguration();
            mathValueConfiguration.Values = new List<MathValue>();
            mathValueConfiguration.Values.Add(load);

            DpidValues dpidValues = new DpidValues();
            dpidValues.Add(rpm, new ParameterValue() { ValueAsString = "1000", ValueAsDouble = 1000.0 });
            dpidValues.Add(maf, new ParameterValue() { ValueAsString = "100", ValueAsDouble = 100.0 });
            
            MathValueProcessor processor = new MathValueProcessor(profile, mathValueConfiguration);
            IEnumerable<string> mathValues = processor.GetMathValues(dpidValues);

            Assert.AreEqual(1, mathValues.Count(), "Number of math values.");
            string loadValue = mathValues.First();
            Assert.AreEqual("6.0", load, "Load value.");
        }
    }
}
