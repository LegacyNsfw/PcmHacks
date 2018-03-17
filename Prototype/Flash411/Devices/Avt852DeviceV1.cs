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
    class Avt852DeviceV1 : Device
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

        public Avt852DeviceV1(IPort port, ILogger logger) : base(port, logger)
        {
        }

        public override string ToString()
        {
            return "AVT 852 V1";
        }

        public override async Task<bool> Initialize()
        {
            this.Logger.AddDebugMessage("Initialize called");
            this.Logger.AddDebugMessage("Initializing " + this.ToString());

            SerialPortConfiguration configuration = new SerialPortConfiguration();
            configuration.BaudRate = 115200;
            await this.Port.OpenAsync(configuration);

            this.Logger.AddDebugMessage("Draining input queue.");
            await this.ProcessIncomingData();

            this.Logger.AddDebugMessage("Sending 'reset' message.");
            await this.Port.Send(Avt852DeviceV1.AVT_RESET);

            if (await ConfirmResponse(AVT_852_IDLE))
            {
                this.Logger.AddUserMessage("AVT device reset ok");
            }
            else
            {
                this.Logger.AddUserMessage("AVT device not found or failed reset");
                this.Logger.AddDebugMessage("Expected " + Avt852DeviceV1.AVT_852_IDLE.ToHex());
                return false;
            }

            await this.Port.Send(Avt852DeviceV1.AVT_ENTER_VPW_MODE);

            if (await ConfirmResponse(AVT_VPW))
            {
                this.Logger.AddUserMessage("AVT device to VPW mode");
            }
            else
            {
                this.Logger.AddUserMessage("Unable to set AVT device to VPW mode");
                this.Logger.AddDebugMessage("Expected " + Avt852DeviceV1.AVT_VPW.ToHex());
                return false;
            }

            return true;
        }

        /// <summary>
        /// This will process incoming messages for up to 500ms, looking for the given message.
        /// </summary>
        private async Task<bool> ConfirmResponse(byte[] expected)
        {
            this.Logger.AddDebugMessage("ConfirmResponse called");
            for (int iterations = 0; iterations < 5; iterations++)
            {
                Queue<Message> queue = await this.ProcessIncomingData();
                foreach (Message message in queue)
                {
                    if (Utility.CompareArrays(message.GetBytes(), expected))
                    {
                        return true;
                    }
                }

                await Task.Delay(100);
            }

            return false;
        }

        /// <summary>
        /// Process incoming data, using a state-machine to parse the incoming messages.
        /// </summary>
        /// <returns></returns>
        private async Task<Queue<Message>> ProcessIncomingData()
        {
            this.Logger.AddDebugMessage("ProcessIncomingData called");
            Queue<Message> queue = new Queue<Message>();

            while (await this.Port.GetReceiveQueueSize() > 0)
            {
                byte[] buffer = ReadAVTPacket();
                {
                    Message message = new Message(buffer);

                    if (message != null)
                    {
                        this.Logger.AddDebugMessage("Received " + message.GetBytes().ToHex());
                        queue.Enqueue(message);
                    }
                }
            }

            return queue;
        }

        private byte[] ReadAVTPacket()
        {
            this.Logger.AddDebugMessage("Trace: ReadAVTPacket");
            int length = 0;
            bool status = true; // do we have a status byte? (we dont for some 9x init commands)

            byte[] rx = new byte[2];

            // Get the first packet byte.
            this.Port.Receive(rx, 0, 1);

            // read an AVT format length
            switch (rx[0])
            {
                case 0x11:
                    this.Port.Receive(rx, 0, 1);
                    length = rx[0];
                    break;
                case 0x12:
                    this.Port.Receive(rx, 0, 1);
                    length = rx[0] << 8;
                    this.Port.Receive(rx, 0, 1);
                    length += rx[0];
                    break;
                case 0x23:
                    this.Port.Receive(rx, 0, 1);
                    if (rx[0] != 0x53)
                    {
                        this.Logger.AddDebugMessage("RX: VPW too long: " + rx[0].ToString("X2"));
                        return new byte[0];
                    }
                    this.Port.Receive(rx, 0, 2);
                    this.Logger.AddDebugMessage("RX: VPW too long and truncated to " + ((rx[0] << 8) + rx[1]).ToString("X4"));
                    length = 4112;
                    break;
                default:
                    this.Logger.AddDebugMessage("default status: " + rx[0].ToString("X2"));
                    length = rx[0] & 0x0F;
                    if ((rx[0] & 0xF0) == 0x90) // special case, init packet with no status
                    {
                        this.Logger.AddDebugMessage("Dont read status, 9X");
                        status = false;
                    }

                    break;
            }

            // if we need to get check and discard the status byte
            if (status == true)
            {
                this.Port.Receive(rx, 0, 1);
                if (rx[0] != 0) this.Logger.AddDebugMessage("RX: bad packet status: " + rx[0].ToString("X2"));
            }

            // return the packet
            byte[] receive = new byte[length];
            Task.Delay(500);
            this.Port.Receive(receive, 0, length);
            this.Logger.AddDebugMessage("Length=" + length + " RX: " + receive.ToHex());

            return rx;
        }

        /// <summary>
        /// Send a message, do not expect a response.
        /// </summary>
        public override Task<bool> SendMessage(Message message)
        {
            this.Logger.AddDebugMessage("Sendmessage called");
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

            this.Logger.AddDebugMessage("Sendrequest called");
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
