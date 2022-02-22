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
            Conversion rpmConversion = new Conversion("RPM", "x", "0");
            LogColumn rpm = new LogColumn(
                new PidParameter(0x3456, "Engine Speed", "", "uint16", false, 
                    new Conversion[] { rpmConversion }),
                rpmConversion);

            Conversion mafConversion = new Conversion("RPM", "x", "0");
            LogColumn maf = new LogColumn(
                new PidParameter(0x1234, "Mass Air Flow", "", "uint16", false,
                    new Conversion[] { mafConversion }),
                mafConversion);

            MathParameter load = new MathParameter(
                "id",
                "Load",
                "",
                new Conversion[] { new Conversion("g/cyl", "(y*60)/x", "0.00") },
                rpm,
                maf);

            LogColumn mathColumn = new LogColumn(load, load.Conversions.First());

            DpidConfiguration profile = new DpidConfiguration();
            profile.ParameterGroups.Add(new ParameterGroup(0xFE));
            profile.ParameterGroups[0].LogColumns.Add(rpm);
            profile.ParameterGroups[0].LogColumns.Add(maf);

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


            PcmParameterValues dpidValues = new PcmParameterValues();
            dpidValues.Add(rpm, new PcmParameterValue() { ValueAsDouble = 1000 });
            dpidValues.Add(maf, new PcmParameterValue() { ValueAsDouble = 100 });

            MathColumnAndDependencies dependencies = new MathColumnAndDependencies(mathColumn, rpm, maf);
            
            MathValueProcessor processor = new MathValueProcessor(profile, new MathColumnAndDependencies[] { dependencies });
            IEnumerable<string> mathValues = processor.GetMathValues(dpidValues);

            Assert.AreEqual(1, mathValues.Count(), "Number of math values.");
            string loadValue = mathValues.First();
            Assert.AreEqual("6.00", loadValue, "Load value.");
        }
    }
}
