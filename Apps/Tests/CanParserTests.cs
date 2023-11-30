using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PcmHacking;

namespace Tests
{
    [TestClass]
    public class CanParserTests
    {
        [TestMethod]
        public void ParseStandardMessage_8bytes()
        {
            var parser = new CanParser();
            byte[] data = { 0xAA, 0xC8, 0x23, 0x01, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x55 };
            CanMessage message;
            for(int index = 0; index < data.Length; index++)
            {
                if (parser.IsCompleteMessage(data[index], out message))
                {
                    Assert.AreEqual((UInt32) 0x123, message.MessageId, "messageId");
                    for (int index2 = 1; index2 <= 8; index2++)
                        Assert.AreEqual(index2 * 0x11, message.Payload[index2 - 1], index2.ToString());
                }
            }
        }

        [TestMethod]
        public void ParseStandardMessage_2bytes()
        {
            var parser = new CanParser();
            byte[] data = { 0xAA, 0xC2, 0x23, 0x01, 0x11, 0x22, 0x55 };
            CanMessage message;
            for (int index = 0; index < data.Length; index++)
            {
                if (parser.IsCompleteMessage(data[index], out message))
                {
                    Assert.AreEqual((UInt32) 0x123, message.MessageId, "messageId");
                    for (int index2 = 1; index2 <= 2; index2++)
                        Assert.AreEqual(index2 * 0x11, message.Payload[index2 - 1], index2.ToString());
                }
            }
        }

        [TestMethod]
        public void ParseExtendedMessage_8bytes()
        {
            var parser = new CanParser();
            byte[] data = { 0xAA, 0xE8, 0x67, 0x45, 0x23, 0x01, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x55 };
            CanMessage message;
            for (int index = 0; index < data.Length; index++)
            {
                if (parser.IsCompleteMessage(data[index], out message))
                {
                    Assert.AreEqual((UInt32) 0x1234567, message.MessageId, "messageId");
                    for (int index2 = 1; index2 <= 8; index2++)
                        Assert.AreEqual(index2 * 0x11, message.Payload[index2 - 1], index2.ToString());
                }
            }
        }

        [TestMethod]
        public void ParseExtendedMessage_2bytes()
        {
            var parser = new CanParser();
            byte[] data = { 0xAA, 0xE2, 0x21, 0x30, 0x03, 0x01, 0x11, 0x22, 0x55 };
            CanMessage message;
            for (int index = 0; index < data.Length; index++)
            {
                if (parser.IsCompleteMessage(data[index], out message))
                {
                    Assert.AreEqual((UInt32) 0x1033021, message.MessageId, "messageId");
                    for (int index2 = 1; index2 <= 8; index2++)
                        Assert.AreEqual(index2 * 0x11, message.Payload[index2 - 1], index2.ToString());
                }
            }
        }
    }
}
