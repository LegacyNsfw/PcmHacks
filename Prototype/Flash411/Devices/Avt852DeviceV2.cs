using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// This class encapsulates all code that is unique to the AVT 852 interface.
    /// </summary>
    /// 
    class Avt852DeviceV2 : Device
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

        public Avt852DeviceV2(IPort port, ILogger logger) : base(port, logger)
        {
        }

        public override string ToString()
        {
            return "AVT 852 V2";
        }

        private AvtStateMachine avtStateMachine;

        private ConcurrentQueue<AvtMessage> queue = new ConcurrentQueue<AvtMessage>();

        /// <summary>
        /// Note that the .Net framework guarantees that only one event handler will execute at a time.
        /// So, this code does not need to support concurrent invocations.
        /// </summary>
        private async void DataReceived(object source, SerialDataReceivedEventArgs args)
        {
            while(await this.Port.GetReceiveQueueSize() > 0)
            {
                byte[] buffer = new byte[await this.Port.GetReceiveQueueSize()];
                int length = await this.Port.Receive(buffer, 0, buffer.Length);
                for(int index = 0; index < length; index++)
                {
                    // If this is null, we're starting a new message.
                    if(this.avtStateMachine == null)
                    {
                        this.avtStateMachine = new AvtStateMachine();
                    }

                    byte value = buffer[index];

                    AvtMessage message = this.avtStateMachine.Push(value);

                    if(message != null)
                    {
                        this.queue.Enqueue(message);
                        this.avtStateMachine = null;
                    }
                }
            }
        }

        public override async Task<bool> Initialize()
        {
            this.Logger.AddDebugMessage("Initializing " + this.ToString());

            SerialPortConfiguration configuration = new SerialPortConfiguration();
            configuration.BaudRate = 115200;
            configuration.DataReceived = this.DataReceived;

            await this.Port.OpenAsync(configuration);

            await this.Port.Send(Avt852DeviceV2.AVT_RESET);

            AvtMessage response = await this.GetResponse();
            if (response == null)
            {
                this.Logger.AddUserMessage("AVT device not found or failed reset. No response.");
                return false;
            }

            byte[] reply = response.Data;

            if (Utility.CompareArrays(reply, Avt852DeviceV2.AVT_852_IDLE))
            {
                this.Logger.AddUserMessage("AVT device reset ok");
            }
            else
            {
                this.Logger.AddUserMessage("AVT device not found of failed reset");
                this.Logger.AddDebugMessage("Expected " + Avt852DeviceV2.AVT_852_IDLE.ToHex());
                this.Logger.AddDebugMessage("Received " + reply);
                return false;
            }

            await this.Port.Send(Avt852DeviceV2.AVT_ENTER_VPW_MODE);

            response = await this.GetResponse();
            if (response == null)
            {
                this.Logger.AddUserMessage("No response when trying to set AVT to VPW mode.");
                return false;
            }

            byte[] reply2 = response.Data;

            if (Utility.CompareArrays(reply2, Avt852DeviceV2.AVT_VPW))
            {
                this.Logger.AddUserMessage("AVT device to VPW mode");

            }
            else
            {
                this.Logger.AddUserMessage("Unable to set AVT device to VPW mode");
                this.Logger.AddDebugMessage("Expected " + Avt852DeviceV2.AVT_VPW.ToHex());
                this.Logger.AddDebugMessage("Received " + reply2);
                return false;
            }

            return true;
        }

        private async Task<AvtMessage> GetResponse()
        {
            AvtMessage response = null;
            for (int iteration = 0; iteration < 10; iteration++)
            {
                if (this.queue.TryDequeue(out response))
                {
                    return response;
                }

                await Task.Delay(100);
            }

            return null;
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
