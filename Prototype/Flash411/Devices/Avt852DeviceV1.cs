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
                Queue<AvtMessage> queue = await this.ProcessIncomingData();
                foreach (AvtMessage message in queue)
                {
                    if (Utility.CompareArrays(message.Data, expected))
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
        private async Task<Queue<AvtMessage>> ProcessIncomingData()
        {
            this.Logger.AddDebugMessage("ProcessIncomingData called");
            Queue<AvtMessage> queue = new Queue<AvtMessage>();

            AvtStateMachine avtStateMachine = new AvtStateMachine();
            while (await this.Port.GetReceiveQueueSize() > 0)
            {
                byte[] buffer = new byte[await this.Port.GetReceiveQueueSize()];
                int length = await this.Port.Receive(buffer, 0, buffer.Length);
                for (int index = 0; index < length; index++)
                {
                    // If this is null, we're starting a new message.
                    if (avtStateMachine == null)
                    {
                        avtStateMachine = new AvtStateMachine();
                    }

                    byte value = buffer[index];

                    AvtMessage message = avtStateMachine.Push(value);

                    if (message != null)
                    {
                        this.Logger.AddDebugMessage("Received " + message.Data.ToHex());
                        queue.Enqueue(message);
                        avtStateMachine = null;
                    }
                }
            }

            return queue;
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
