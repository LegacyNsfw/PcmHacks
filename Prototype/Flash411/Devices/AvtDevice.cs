using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    class AvtDevice : Device
    {
        public const string DeviceType = "AVT (842/852)";

        public static readonly Message AVT_RESET            = new Message(new byte[] { 0xF1, 0xA5 });
        public static readonly Message AVT_ENTER_VPW_MODE   = new Message(new byte[] { 0xE1, 0x33 });
        public static readonly Message AVT_REQUEST_MODEL    = new Message(new byte[] { 0xF0 });
        public static readonly Message AVT_REQUEST_FIRMWARE = new Message(new byte[] { 0xB0 });

        // AVT reader strips the header
        public static readonly Message AVT_VPW              = new Message(new byte[] { 0x07 });       // 91 01
        public static readonly Message AVT_852_IDLE         = new Message(new byte[] { 0x27 });       // 91 27
        public static readonly Message AVT_842_IDLE         = new Message(new byte[] { 0x12 });       // 91 12
        public static readonly Message AVT_FIRMWARE         = new Message(new byte[] { 0x04 });       // 92 04 15 (firmware 1.5)
        public static readonly Message AVT_TX_ACK           = new Message(new byte[] { 0x60 });       // 01 60
        public static readonly Message AVT_BLOCK_TX_ACK     = new Message(new byte[] { 0xF3, 0x60 }); // F3 60

        public AvtDevice(IPort port, ILogger logger) : base(port, logger)
        {
        }

        public override string ToString()
        {
            return DeviceType;
        }

        public override async Task<bool> Initialize()
        {
            this.Logger.AddDebugMessage("Initialize called");
            this.Logger.AddDebugMessage("Initializing " + this.ToString());

            Response<Message> m; // hold returned messages for processing

            SerialPortConfiguration configuration = new SerialPortConfiguration();
            configuration.BaudRate = 115200;
            await this.Port.OpenAsync(configuration);

            this.Logger.AddDebugMessage("Flushing serial buffers");
            await this.Port.DiscardBuffers();

            this.Logger.AddDebugMessage("Sending 'reset' message.");
            await this.Port.Send(AvtDevice.AVT_RESET.GetBytes());
            m = await this.FindResponse(AVT_852_IDLE);
            if (m.Status == ResponseStatus.Success )
            {
                this.Logger.AddUserMessage("AVT device reset ok");
            }
            else
            {
                this.Logger.AddUserMessage("AVT device not found or failed reset");
                this.Logger.AddDebugMessage("Expected " + AvtDevice.AVT_852_IDLE.GetString());
                return false;
            }

            this.Logger.AddDebugMessage("Looking for Firmware message");
            m = await this.FindResponse(AVT_FIRMWARE);
            if ( m.Status == ResponseStatus.Success )
            {
                byte firmware = m.Value.GetBytes()[1];
                int major = firmware >> 4;
                int minor = firmware & 0x0F;
                this.Logger.AddUserMessage("AVT Firmware " + major + "." + minor);
            }
            else
            {
                this.Logger.AddUserMessage("Firmware not found or failed reset");
                this.Logger.AddDebugMessage("Expected " + AVT_FIRMWARE.GetBytes());
                return false;
            }

            await this.Port.Send(AvtDevice.AVT_ENTER_VPW_MODE.GetBytes());
            m = await FindResponse(AVT_VPW);
            if (m.Status == ResponseStatus.Success)
            {
                this.Logger.AddDebugMessage("Set VPW Mode");
                this.Logger.AddUserMessage("Set VPW Mode");
            }
            else
            {
                this.Logger.AddUserMessage("Unable to set AVT device to VPW mode");
                this.Logger.AddDebugMessage("Expected " + AvtDevice.AVT_VPW.GetString());
                return false;
            }

            await AVTSetup();

            return true;
        }

        /// <summary>
        /// This will process incoming messages for up to 500ms looking for a message
        /// </summary>
        public async Task<Response<Message>> FindResponse(Message expected)
        {
            this.Logger.AddDebugMessage("ConfirmResponse called");
            for (int iterations = 0; iterations < 5; iterations++)
            {
                Response<Message> response = await this.ReadAVTPacket();
                if (response.Status == ResponseStatus.Success) 
                    if (Utility.CompareArraysPart(response.Value.GetBytes(), expected.GetBytes()))
                        return Response.Create(ResponseStatus.Success, (Message) response.Value);
                await Task.Delay(100);
            }

            return Response.Create(ResponseStatus.Timeout, (Message) null);
        }

        /// <summary>
        /// Read an AVT formatted packet from the interface, and return a Response/Message
        /// </summary>
        async private Task<Response<Message>> ReadAVTPacket()
        {
            this.Logger.AddDebugMessage("Trace: ReadAVTPacket");
            int length = 0;
            bool status = true; // do we have a status byte? (we dont for some 9x init commands)
            byte[] rx = new byte[2]; // we dont read more than 2 bytes at a time

            // Get the first packet byte.
            await this.Port.Receive(rx, 0, 1);

            // read an AVT format length
            switch (rx[0])
            {
                case 0x11:
                    await this.Port.Receive(rx, 0, 1);
                    length = rx[0];
                    this.Logger.AddDebugMessage("RX: AVT Type 11. Length  " + rx[0].ToString("X2"));
                    break;
                case 0x12:
                    await this.Port.Receive(rx, 0, 1);
                    length = rx[0] << 8;
                    await this.Port.Receive(rx, 0, 1);
                    length += rx[0];
                    this.Logger.AddDebugMessage("RX: AVT Type 12. Length  " + rx[0].ToString("X2"));
                    break;
                case 0x23:
                    this.Logger.AddDebugMessage("RX: AVT Type 23");
                    await this.Port.Receive(rx, 0, 1);
                    if (rx[0] != 0x53)
                    {
                        this.Logger.AddDebugMessage("RX: VPW too long: " + rx[0].ToString("X2"));
                        return Response.Create(ResponseStatus.Error, (Message)null);
                    }
                    await this.Port.Receive(rx, 0, 2);
                    int trunclen = ((rx[0] << 8) + rx[1]);
                    this.Logger.AddDebugMessage("RX: VPW too long and truncated to " + trunclen.ToString("X4"));
                    length = trunclen;
                    break;
                default:
                    this.Logger.AddDebugMessage("RX: Header " + rx[0].ToString("X2"));
                    int type = rx[0] >> 4;
                    switch (type) {
                        case 0: // standard < 16 byte data packet
                            length = rx[0] & 0x0F;
                            this.Logger.AddDebugMessage("RX: AVT Type 0 (with status) length " + length);
                            break;
                        case 6: // avt filter
                        case 9: // init and version
                            length = rx[0] & 0x0F;
                            status = false;
                            this.Logger.AddDebugMessage("RX: AVT Type " + type + " (no status) length " + length);
                            break;
                        default:
                            this.Logger.AddDebugMessage("RX: Unhandled packet type " + type + ". Add support to ReadAVTPacket()");
                            break;
                    }
                    break;
            }

            // if we need to get check and discard the status byte
            if (status == true)
            {
                length--;
                await this.Port.Receive(rx, 0, 1);
                if (rx[0] != 0) this.Logger.AddDebugMessage("RX: bad packet status: " + rx[0].ToString("X2"));
            }

            // return the packet
            byte[] receive = new byte[length];
            await this.Port.Receive(receive, 0, length);
            this.Logger.AddDebugMessage("Length=" + length + " RX: " + receive.ToHex());

            return Response.Create(ResponseStatus.Success, new Message(receive));
        }

        /// <summary>
        /// Convert a Message to an AVT formatted transmit, and send to the interface
        /// </summary>
        async private Task<Response<Message>> SendAVTPacket(Message message)
        {
            this.Logger.AddDebugMessage("Trace: SendAVTPacket");

            byte[] txb = { 0x12 };
            int length = message.GetBytes().Length;

            if (length > 0xFF)
            {
                await this.Port.Send(txb);
                txb[0] = unchecked((byte)(length >> 8));
                await this.Port.Send(txb);
                txb[0] = unchecked((byte)(length & 0xFF));
                await this.Port.Send(txb);
            }
            else if (length > 0x0F)
            {
                txb[0] = (byte)(0x11);
                await this.Port.Send(txb);
                txb[0] = unchecked((byte)(length & 0xFF));
                await this.Port.Send(txb);
            }
            else
            {
                txb[0] = unchecked((byte)(length & 0x0F));
                await this.Port.Send(txb);
            }

            await this.Port.Send(message.GetBytes());

            return Response.Create(ResponseStatus.Success, message);
        }

        /// <summary>
        /// Configure AVT to return only packets targeted to the tool (Device ID F0), and disable transmit acks
        /// </summary>
        async private Task<Response<Boolean>> AVTSetup()
        {
            this.Logger.AddDebugMessage("AVTFilter called");

            byte[] filterdest = { 0x52, 0x5B, 0xF0 };
            byte[] filterdestok = { 0x5B, 0xF0 }; // 62 5B F0
            byte[] disabletxack = { 0x52, 0x40, 0x00 };
            byte[] disabletxackok = { 0x40, 0x00 }; // 62 40 00

            await this.Port.Send(disabletxack);
            Response<Message> response = await ReadAVTPacket();

            if (Utility.CompareArraysPart(response.Value.GetBytes(), disabletxackok))
            {
                this.Logger.AddDebugMessage("AVT Acks disabled");
            }
            else
            {
                this.Logger.AddDebugMessage("Failed to disable AVT Acks");
                return Response.Create(ResponseStatus.Error, false);
            }

            await this.Port.Send(filterdest);
            response = await ReadAVTPacket();

            if (Utility.CompareArraysPart(response.Value.GetBytes(), filterdestok))
            {
                this.Logger.AddDebugMessage("AVTFilter enabled");
                return Response.Create(ResponseStatus.Success, true);
            }
            else
            {
                this.Logger.AddDebugMessage("Failed to set AVT Filter");
                return Response.Create(ResponseStatus.Error, false);
            }
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
        public override async Task<Response <Message>> SendRequest(Message message)
        {
            this.Logger.AddDebugMessage("Sendrequest called");
            this.Logger.AddDebugMessage("TX: " + message.GetBytes().ToHex());
            await SendAVTPacket(message);

            Response<Message> response = await ReadAVTPacket();
            if (response.Status != ResponseStatus.Success) return response;

            this.Logger.AddDebugMessage("RX: " + response.Value.GetBytes().ToHex());

            return response;
        }
    }
}
