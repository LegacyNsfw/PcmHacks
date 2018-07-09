using System;
using System.Text;
using System.Threading.Tasks;

using PcmHacking;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class AvtTests
    {
        [TestMethod]
        public async Task SendRequest()
        {
            // Create the object we're going to test.
            TestLogger logger = new TestLogger();
            TestPort port = new TestPort(logger);
            AvtDevice device = new AvtDevice(port, logger);

            // Specify the sequence of bytes that we would expect to get back from the serial port.
            port.EnqueueBytes(new byte[] { 0x11, 0x0C, 0x00, 0x6c, 0xF0, 0x10, 0x7C, 0x01, 0x00, 0x31, 0x47, 0x31, 0x59, 0x59, 0xC3 });
            port.BytesToReceive.Position = 0;
            
            // Send a message.
            Message message = new Message(new byte[] { 0x6c, 0x10, 0xF0, 0x3C, 0x01 });
            bool sendSuccess = await device.SendMessage(message);

            // Confirm success.
            Assert.IsTrue(sendSuccess, "Send success");

            // Confirm that the device sent the bytes we expect it to send.
            // The 00 00 00 stuff is just placeholders for real data.
            Assert.AreEqual("05", port.MessagesSent[0].ToHex(), "First command");
            Assert.AreEqual(message.GetBytes().ToHex(), port.MessagesSent[1].ToHex(), "Second command");

            // Confirm that the device interpreted the response as expected.
            Message response = await device.ReceiveMessage();

            Assert.IsNotNull(response, "Response should not be null.");
            Assert.AreEqual("6C F0 10 7C 01 00 31 47 31 59 59", response.GetBytes().ToHex(), "Response message");
        }
    }
}
