using System;
using System.Text;
using System.Threading.Tasks;

using Flash411;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class MockDeviceTests
    {
        [TestMethod]
        public async Task SendRequest()
        {
            // Create the object we're going to test.
            TestLogger logger = new TestLogger();
            TestPort port = new TestPort(logger);
            MockDevice device = new MockDevice(port, logger);

            // Specify the sequence of bytes that we would expect to get back from the serial port.
            port.EnqueueBytes(new byte[] { 0x6c, 0xF0, 0x10, 0x7C, 0x01, 0x00, 0x31, 0x47, 0x31, 0x59, 0x59, 0xC3 });
            port.BytesToReceive.Position = 0;
            
            // Send a message.
            Message message = new Message(new byte[] { 0x6c, 0x10, 0xF0, 0x3C, 0x01 });
            await device.SendMessage(message);
            Message response = await device.ReceiveMessage();
            
            // Confirm that the device sent the bytes we expect it to send.
            Assert.AreEqual(message.GetBytes().ToHex(), port.MessagesSent[0].ToHex(), "Second command");

            // Confirm that the device interpreted the response as expected.
            Assert.AreEqual("6C F0 10 7C 01 00 31 47 31 59 59 C3", response.GetBytes().ToHex(), "Response message");
        }
    }
}
