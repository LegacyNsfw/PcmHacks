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
            Conversion rpmConversion = new Conversion("RPM", "X", "0");
            ProfileParameter rpm = new ProfileParameter(
                new Parameter("RPM", 0x3456, "Engine Speed", "", 2, false, 
                    new Conversion[] { rpmConversion }),
                rpmConversion);

            Conversion mafConversion = new Conversion("RPM", "X", "0");
            ProfileParameter maf = new ProfileParameter(
                new Parameter("MAF", 0x1234, "Mass Air Flow", "", 2, false,
                    new Conversion[] { mafConversion }),
                mafConversion);

            MathValue load = new MathValue();
            load.XParameter = rpm.Parameter.Name;
            load.XConversion = rpm.Conversion.Units;
            load.YParameter = maf.Parameter.Name;
            load.YConversion = maf.Conversion.Units;
            load.Format = "0.00";
            load.Formula = "(y*60)/x";

            DpidConfiguration profile = new DpidConfiguration();
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
            mathValueConfiguration.MathValues = new List<MathValue>();
            mathValueConfiguration.MathValues.Add(load);

            DpidValues dpidValues = new DpidValues();
            dpidValues.Add(rpm, new ParameterValue() { RawValue = 1000 });
            dpidValues.Add(maf, new ParameterValue() { RawValue = 100 });
            
            MathValueProcessor processor = new MathValueProcessor(profile, mathValueConfiguration);
            IEnumerable<string> mathValues = processor.GetMathValues(dpidValues);

            Assert.AreEqual(1, mathValues.Count(), "Number of math values.");
            string loadValue = mathValues.First();
            Assert.AreEqual("6.00", loadValue, "Load value.");
        }
    }
}
