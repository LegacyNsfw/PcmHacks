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
        public static readonly Message AVT_RESET = new Message(new byte[] { 0xF1, 0xA5 });
        public static readonly Message AVT_ENTER_VPW_MODE = new Message(new byte[] { 0xE1, 0x33 });
        public static readonly Message AVT_REQUEST_MODEL = new Message(new byte[] { 0xF0 });
        public static readonly Message AVT_REQUEST_FIRMWARE = new Message(new byte[] { 0xB0 });

        public static readonly Message AVT_VPW = new Message(new byte[] { 0x07 }); // 91 07
        public static readonly Message AVT_852_IDLE = new Message(new byte[] { 0x27 }); // 91 27
        public static readonly Message AVT_842_IDLE = new Message(new byte[] { 0x12 }); // 92 12
        public static readonly Message AVT_TX_ACK = new Message(new byte[] { 0x60 }); // 01 60
        public static readonly Message AVT_BLOCK_TX_ACK = new Message(new byte[] { 0x60 }); // F3 60 XX XX

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
            this.Logger.AddDebugMessage("Trace: DataReceived");
            while (await this.Port.GetReceiveQueueSize() > 0)
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
            byte[] response= { 0 };

            await this.Port.OpenAsync(configuration);

            await SendRequest(Avt852DeviceV2.AVT_RESET);

            if (response == null)
            {
                this.Logger.AddUserMessage("AVT device not found or failed reset. No response.");
                return false;
            }
            /*
            byte[] reply = response.Data;

            if (Utility.CompareArrays(reply, Avt852DeviceV2.AVT_852_IDLE.GetBytes()))
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

            await Task.Delay(100);

            await this.Port.Send(Avt852DeviceV2.AVT_ENTER_VPW_MODE);

            response = await this.GetResponse();
            if (response == null)
            {
                this.Logger.AddUserMessage("No response when trying to set AVT to VPW mode.");
                return false;
            }

            byte[] reply2 = response.Data;

            if (Utility.CompareArrays(reply2, Avt852DeviceV2.AVT_VPW.GetBytes()))
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
            */
            return true;
        }

        private async Task<AvtMessage> GetResponse()
        {
            this.Logger.AddDebugMessage("Trace: GetResponse");
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
            this.Logger.AddDebugMessage("Trace: SendMessage");
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
            this.Logger.AddDebugMessage("Trace: SendRequest");
            int length = 0;
            bool status = true; // do we have a status byte? (we dont for some 9x init commands)
            StringBuilder builder = new StringBuilder();
            this.Logger.AddDebugMessage("TX: " + message.GetBytes().ToHex());
            this.Port.Send(message.GetBytes());
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
                        return Task.FromResult(Response.Create(ResponseStatus.Error, rx));
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

            return Task.FromResult(Response.Create(ResponseStatus.Success, receive));
        }
    }
}
