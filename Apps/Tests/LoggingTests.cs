using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PcmHacking;

namespace Tests
{
    [TestClass]
    public class LoggingTests
    {
        private Dictionary<uint, uint> GetAddress(uint address)
        {
            Dictionary<uint, uint> addresses = new Dictionary<uint, uint>();
            addresses[0] = address;
            return addresses;
        }

        [TestMethod]
        public void DecodeDpid()
        {
            Conversion conversion = new Conversion("units", "x*0.9", "0.00");
            Conversion[] conversions = new Conversion[] { conversion };

            Parameter signed8 =    new RamParameter("S8",  "signed 8-bit",    "", "int8",   false, conversions, GetAddress(0));
            Parameter unsigned8 =  new RamParameter("U8",  "unsigned 8-bit",  "", "uint8",  false, conversions, GetAddress(0));
            Parameter signed16 =   new RamParameter("U8",  "signed 16-bit",   "", "int16",  false, conversions, GetAddress(0));
            Parameter unsigned16 = new RamParameter("U16", "unsigned 16-bit", "", "uint16", false, conversions, GetAddress(0));

            LogColumn signed8Column = new LogColumn(signed8, conversion);
            LogColumn unsigned8Column = new LogColumn(unsigned8, conversion);
            LogColumn signed16Column = new LogColumn(signed16, conversion);
            LogColumn unsigned16Column = new LogColumn(unsigned16, conversion);

            ParameterGroup group = new ParameterGroup(0);

            group.TryAddLogColumn(signed8Column);
            group.TryAddLogColumn(unsigned8Column);
            group.TryAddLogColumn(signed16Column);
            group.TryAddLogColumn(unsigned16Column);

            DpidConfiguration dpid = new DpidConfiguration();
            dpid.TryAddGroup(group);
            LogRowParser parser = new LogRowParser(dpid);

            Int16 int16 = -3000;
            UInt16 uint16 = 50000;
            byte[] bytesOfSigned16 =   new byte[] { (byte) ((int16 & 0xFF00) >> 8), (byte) (int16 & 0xFF) };
            byte[] bytesOfUnsigned16 = new byte[] { (byte) ((uint16 & 0xFF00) >> 8), (byte) (uint16 & 0xFF) };

            byte signed8Byte;
            unchecked
            {
                signed8Byte = (byte)-20;
            }

            RawLogData data = new RawLogData(
                0,
                new byte[] 
                {
                    signed8Byte,
                    200,
                    bytesOfSigned16[0],
                    bytesOfSigned16[1],
                    bytesOfUnsigned16[0],
                    bytesOfUnsigned16[1]
                });

            parser.ParseData(data);
            PcmParameterValues values = parser.Evaluate();

            // Values are scaled here to match the conversion formula.
            Assert.AreEqual(-20 * 0.9, values[signed8Column].ValueAsDouble, "signed 8");
            Assert.AreEqual(200 * 0.9, values[unsigned8Column].ValueAsDouble, "unsigned 8");
            Assert.AreEqual(int16 * 0.9, values[signed16Column].ValueAsDouble, "signed 16");
            Assert.AreEqual(uint16 * 0.9, values[unsigned16Column].ValueAsDouble, "unsigned 16");
        }
    }
}
