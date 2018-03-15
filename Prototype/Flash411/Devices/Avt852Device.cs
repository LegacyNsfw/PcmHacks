using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// This class encapsulates all code that is unique to the AVT 852 interface.
    /// </summary>
    /// 
    class Avt852Device : Device
    {
        public static readonly byte[] AVT_RESET = { 0xF1, 0xA5 };
        public static readonly byte[] AVT_ENTER_VPW_MODE = { 0xE1, 0x33 };
        public static readonly byte[] AVT_REQUEST_MODEL = { 0xF0 };
        public static readonly byte[] AVT_REQUEST_FIRMWARE = { 0xB0 };

        public static readonly byte[] AVT_VPW = { 0x91, 0x07 };
        public static readonly byte[] AVT_852_IDLE = { 0x91, 0x27 };
        public static readonly byte[] AVT_842_IDLE = { 0x91, 0x12 };
        public static readonly byte[] AVT_TX_ACK = { 0x01, 0x60 };
        public static readonly byte[] AVT_BLOCK_TX_ACK = { 0xF3, 0x60 };

        public Avt852Device(IPort port, ILogger logger) : base(port, logger)
        {
        }

        public override string ToString()
        {
            return "AVT 852";
        }

        public override async Task<bool> Initialize()
        {
            this.Logger.AddDebugMessage("Initializing " + this.ToString());

            SerialPortConfiguration configuration = new SerialPortConfiguration();
            configuration.BaudRate = 115200;
            await this.Port.OpenAsync(configuration);

            await this.Port.Send(Avt852Device.AVT_RESET);
            byte[] reply = await ReceiveBuffer();

            if (Utility.CompareArrays(reply, Avt852Device.AVT_852_IDLE))
            {
                this.Logger.AddUserMessage("AVT device reset ok");
            }
            else
            {
                this.Logger.AddUserMessage("AVT device not found of failed reset");
                this.Logger.AddDebugMessage("Expected " + Avt852Device.AVT_852_IDLE.ToHex());
                this.Logger.AddDebugMessage("Received " + reply);
                return false;
            }

            await this.Port.Send(Avt852Device.AVT_ENTER_VPW_MODE);
            byte[] reply2 = await ReceiveBuffer();

            if (Utility.CompareArrays(reply2, Avt852Device.AVT_VPW))
            {
                this.Logger.AddUserMessage("AVT device to VPW mode");

            }
            else
            {
                this.Logger.AddUserMessage("Unable to set AVT device to VPW mode");
                this.Logger.AddDebugMessage("Expected " + Avt852Device.AVT_VPW.ToHex());
                this.Logger.AddDebugMessage("Received " + reply2);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create a complete message from the AVT 852.
        /// </summary>
        /// <returns></returns>
        private async Task<byte[]> ReceiveBuffer()
        {
            byte[] header = new byte[1];
            int receiveLength = 0;

            for (int iterations = 0; iterations < 5; iterations++)
            {
                // Read the header byte
                receiveLength = await this.Port.Receive(header, 0, 1);
                if (receiveLength == 0)
                {
                    await Task.Delay(100);
                    continue;
                }

                break;
            }

            // Find out how many bytes are expected
            int expected = header[0] & 0x0F;

            // Loop around port.Receive until all expected bytes are recevied
            List<byte> buffer = new List<byte>(expected);
            for (int iterations = 0; iterations < 10; iterations++)
            {
                byte[] segment = new byte[expected - buffer.Count];
                receiveLength = await this.Port.Receive(segment, 0, segment.Length);

                // If no bytes came back, pause and try again
                if (receiveLength == 0)
                {
                    await Task.Delay(100);
                    continue;
                }
                // Add the bytes from this segment to the buffer
                buffer.AddRange(segment.Take(receiveLength));

                if (buffer.Count >= expected)
                {
                    break;
                }
            }

            return header.Concat(buffer).ToArray();
        }

        /// <summary>
        /// Send a message, do not expect a response.
        /// </summary>
        public override Task<bool> SendMessage(Message message)
        {
            StringBuilder builder = new StringBuilder();
            this.Logger.AddDebugMessage("Sending message " + message.GetBytes().ToHex());
            this.Port.Send(message.GetBytes());
            return Task.FromResult(true);
        }

        /// <summary>
        /// Send a message, wait for a response, return the response.
        /// </summary>
        public override Task<Response<byte[]>> SendRequest(Message message)
        {

            StringBuilder builder = new StringBuilder();
            this.Logger.AddDebugMessage("TX: " + message.GetBytes().ToHex());
            this.Port.Send(message.GetBytes());

            // This code here will need to handle AVT packet formatting
            byte[] response = new byte[100];
            this.Port.Receive(response, 0, 2);

            this.Logger.AddDebugMessage("RX: " + message.GetBytes().ToHex());
            this.Port.Send(message.GetBytes());

            return Task.FromResult(Response.Create(ResponseStatus.Success, response));
        }
    }
}
