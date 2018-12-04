using System;
using System.Text;
using System.Threading.Tasks;

using PcmHacking;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class ScanToolTests
    {
        [TestMethod]
        public async Task SendRequest()
        {
            // Create the object we're going to test.
            TestLogger logger = new TestLogger();
            TestPort port = new TestPort(logger);
            Message response = null;
            Action<Message> responseSetter = (x) => { response = x; };
            ScanToolDeviceImplementation device = new ScanToolDeviceImplementation(responseSetter, null, port, logger);

            // Specify the sequence of bytes that we would expect to get back from the serial port.
            // Note that the test passes, but the first sequence ends with ">\r\n" and the second ends with "\r\n>"  - this seems suspicious.
            port.EnqueueBytes(Encoding.ASCII.GetBytes("OK>\r\n"));
            port.EnqueueBytes(Encoding.ASCII.GetBytes("6CF0107C01003147315959C3\r\n>"));
            port.BytesToReceive.Position = 0;

            // Send a message.
            Message message = new Message(new byte[] { 0x6c, 0x10, 0xF0, 0x3C, 0x01 });
            bool sendSuccess = await device.SendMessage(message);

            // Confirm success.
            Assert.IsTrue(sendSuccess, "Send success.");

            // Confirm that the device sent the bytes we expect it to send.
            Assert.AreEqual("AT SH 6C 10 F0 \r\n", System.Text.Encoding.ASCII.GetString(port.MessagesSent[0]), "Set-header command");
            Assert.AreEqual("3C 01\r\n", Encoding.ASCII.GetString(port.MessagesSent[1]), "Read block 1 command");

            // Confirm that the device interpreted the response as expected.
            Assert.IsNotNull(response, "Response should not be null.");
            Assert.AreEqual("6C F0 10 7C 01 00 31 47 31 59 59", response.GetBytes().ToHex(), "Response message");
        }
    }
}
