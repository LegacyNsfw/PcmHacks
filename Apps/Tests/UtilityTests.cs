using System;
using PcmHacking;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void ToHex()
        {
            Assert.AreEqual("", new byte[0].ToHex(), "Zero length");
            Assert.AreEqual("01", new byte[] { 0x01 }.ToHex(), "One byte");
            Assert.AreEqual("01 FF", new byte[] { 0x01, 0xFF }.ToHex(), "Two bytes");
            Assert.AreEqual("00 01 99 AA FF", new byte[] { 0x00, 0x01, 0x99, 0xAA, 0xFF }.ToHex(), "Five bytes");
        }

        [TestMethod]
        public void ToHexWithCount()
        {
            Assert.AreEqual("", new byte[0].ToHex(0), "Zero length, count=0");
            Assert.AreEqual("01", new byte[] { 0x01 }.ToHex(2), "One byte, count=2");
            Assert.AreEqual("01", new byte[] { 0x01, 0xFF }.ToHex(1), "Two bytes, count=1");
            Assert.AreEqual("00 01", new byte[] { 0x00, 0x01, 0x99, 0xAA, 0xFF }.ToHex(2), "Five bytes, count=2");
        }

        [TestMethod]
        public void CompareArrays()
        {
            Assert.IsTrue(Utility.CompareArrays(new byte[0], new byte[0]), "Zero length");
            Assert.IsTrue(Utility.CompareArrays(new byte[] { 0x01 }, new byte[] { 0x01 }), "One byte");
            Assert.IsTrue(Utility.CompareArrays(new byte[] { 0x01, 0x02, 0x03 }, new byte[] { 0x01, 0x02, 0x03 }), "Three bytes");
            Assert.IsFalse(Utility.CompareArrays(new byte[] { 0x01, 0x02, 0x03 }, new byte[] { 0x01, 0x02 }), "2nd array too short");
            Assert.IsFalse(Utility.CompareArrays(new byte[] { 0x01, 0x02 }, new byte[] { 0x01, 0x02, 0x03 }), "2nd array too long");
            Assert.IsFalse(Utility.CompareArrays(new byte[] { 0x01, 0x02, 0x03 }, new byte[] { 0x01, 0x02, 0xFF }), "Mismatched byte");
        }
    }
}
