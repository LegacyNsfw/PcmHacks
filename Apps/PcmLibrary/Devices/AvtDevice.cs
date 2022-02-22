using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// This class encapsulates all code that is unique to the AVT 852 interface.
    /// </summary>
    /// 
    public class AvtDevice : SerialDevice
    {
        public const string DeviceType = "AVT (838/842/852)";
        public short Model = 0; // 0 = unknown or 838, 842, 852

        public static readonly Message AVT_RESET                = new Message(new byte[] { 0xF1, 0xA5 });
        public static readonly Message AVT_ENTER_VPW_MODE       = new Message(new byte[] { 0xE1, 0x33 });
        public static readonly Message AVT_REQUEST_MODEL        = new Message(new byte[] { 0xF0 });
        public static readonly Message AVT_REQUEST_FIRMWARE     = new Message(new byte[] { 0xB0 });
        public static readonly Message AVT_DISABLE_TX_ACK       = new Message(new byte[] { 0x52, 0x40, 0x00 });
        public static readonly Message AVT_FILTER_DEST          = new Message(new byte[] { 0x52, 0x5B, DeviceId.Tool });
        public static readonly Message AVT_1X_SPEED             = new Message(new byte[] { 0xC1, 0x00 });
        public static readonly Message AVT_4X_SPEED             = new Message(new byte[] { 0xC1, 0x01 });

        // AVT reader strips the header
        public static readonly Message AVT_VPW                  = new Message(new byte[] { 0x07 });       // 91 07
        public static readonly Message AVT_852_IDLE             = new Message(new byte[] { 0x27 });       // 91 27
        public static readonly Message AVT_842_IDLE             = new Message(new byte[] { 0x12 });       // 91 12
        public static readonly Message AVT_FIRMWARE             = new Message(new byte[] { 0x04 });       // 92 04 15 (firmware 1.5)
        public static readonly Message AVT_TX_ACK               = new Message(new byte[] { 0x60 });       // 01 60
        public static readonly Message AVT_FILTER_DEST_OK       = new Message(new byte[] { 0x5B, DeviceId.Tool });// 62 5B F0
        public static readonly Message AVT_DISABLE_TX_ACK_OK    = new Message(new byte[] { 0x40, 0x00 }); // 62 40 00
        public static readonly Message AVT_BLOCK_TX_ACK         = new Message(new byte[] { 0xF3, 0x60 }); // F3 60

        public AvtDevice(IPort port, ILogger logger) : base(port, logger)
        {
            this.MaxSendSize = 4096+10+2;    // packets up to 4112 but we want 4096 byte data blocks
            this.MaxReceiveSize = 4096+10+2; // with 10 byte header and 2 byte block checksum
            this.Supports4X = true;
            this.SupportsSingleDpidLogging = true;
            this.SupportsStreamLogging = true;
        }

        public override string GetDeviceType()
        {
            return DeviceType;
        }

        public override async Task<bool> Initialize()
        {
            this.Logger.AddDebugMessage("Initializing " + this.ToString());

            Response<Message> m;

            SerialPortConfiguration configuration = new SerialPortConfiguration();
            configuration.BaudRate = 57600; // default RS232 speed for 838, 842. ignored by the USB 852.
            await this.Port.OpenAsync(configuration);
            await this.Port.DiscardBuffers();

            this.Logger.AddDebugMessage("Sending 'reset' message.");
            await this.Port.Send(AvtDevice.AVT_RESET.GetBytes());
            m = await ReadAVTPacket();
            if (m.Status == ResponseStatus.Success)
            {
                switch (m.Value.GetBytes()[0])
                {
                    case 0x27:
                        this.Logger.AddUserMessage("AVT 852 Reset OK");
                        this.Model = 852;
                        break;
                    case 0x12:
                        this.Logger.AddUserMessage("AVT 842 Reset OK");
                        this.Model = 842;
                        break;
                    case 0x07:
                        this.Logger.AddUserMessage("AVT 838 Reset OK");
                        this.Model = 838;
                        this.MaxSendSize = 2048 + 10 + 2;
                        this.MaxReceiveSize = 2048 + 10 + 2;
                        break;
                    default:
                        this.Logger.AddUserMessage("Unknown and unsupported AVT device detected. Please add support and submit a patch!");
                        return false;
                }
            }
            else
            {
                this.Logger.AddUserMessage("AVT device not found or failed reset");
                return false;
            }

            this.Logger.AddDebugMessage("Looking for Firmware message");
            if (this.Model == 838)
            {
                await this.Port.Send(AvtDevice.AVT_REQUEST_FIRMWARE.GetBytes()); // we need to request this on 838 but the 852 sends it without being asked. 842 needs testing.
            }

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

            // 838 defaults to vpw mode, so dont set it on that device.
            if (this.Model != 838) {
                await this.Port.Send(AvtDevice.AVT_ENTER_VPW_MODE.GetBytes());
                m = await FindResponse(AVT_VPW);
                if (m.Status == ResponseStatus.Success)
                {
                    this.Logger.AddDebugMessage("Set VPW Mode");
                }
                else
                {
                    this.Logger.AddUserMessage("Unable to set AVT device to VPW mode");
                    this.Logger.AddDebugMessage("Expected " + AvtDevice.AVT_VPW.ToString());
                    return false;
                }
            }

            await AVTSetup();

            return true;
        }

        /// <summary>
        /// Not yet implemented.
        /// </summary>
        public override Task<TimeoutScenario> SetTimeout(TimeoutScenario scenario)
        {
            return Task.FromResult(this.currentTimeoutScenario);
        }

        /// <summary>
        /// This will process incoming messages for up to 500ms looking for a message
        /// </summary>
        public async Task<Response<Message>> FindResponse(Message expected)
        {
            //this.Logger.AddDebugMessage("FindResponse called");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while(stopwatch.ElapsedMilliseconds < 3000)
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

            //this.Logger.AddDebugMessage("Trace: ReadAVTPacket");
            int length = 0;
            bool status = true; // do we have a status byte? (we dont for some 9x init commands)
            byte[] rx = new byte[2]; // we dont read more than 2 bytes at a time

            // Get the first packet byte.
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                while (sw.ElapsedMilliseconds < 1000)
                {
                    if (await this.Port.GetReceiveQueueSize() > 0) { break;}
                }
                if (await this.Port.GetReceiveQueueSize() > 0)
                {
                    await this.Port.Receive(rx, 0, 1);
                }
                else
                {
                    this.Logger.AddDebugMessage("Waited 2seconds.. no data present");
                    return Response.Create(ResponseStatus.Timeout, (Message)null);
                }
            }
            catch (Exception) // timeout exception - log no data, return error.
            {
                this.Logger.AddDebugMessage("No Data");
                return Response.Create(ResponseStatus.Timeout, (Message)null);
            }

            // read an AVT format length
            switch (rx[0])
            {
                case 0x11:
                    await this.Port.Receive(rx, 0, 1);
                    length = rx[0];
                    break;
                case 0x12:
                    await this.Port.Receive(rx, 0, 1);
                    length = rx[0] << 8;
                    await this.Port.Receive(rx, 0, 1);
                    length += rx[0];
                    break;
                default:
                    //this.Logger.AddDebugMessage("RX: Header " + rx[0].ToString("X2"));
                    int type = rx[0] >> 4;
                    switch (type) {
                        case 0xF: // standard < 16 byte data packet (AVT 838)
                        case 0x0: // standard < 16 byte data packet (AVT 842/852)
                            length = rx[0] & 0x0F;
                            break;
                        case 0x2:
                            length = rx[0] & 0x0F;
                            status = false;
                            break;
                        case 0x3: // Invalid Command
                            length = rx[0] & 0x0F;
                            byte[] r = new byte[length];
                            await this.Port.Receive(r, 0, 1);
                            this.Logger.AddDebugMessage("RX: Invalid command. Packet that began with  " + r.ToHex() + " was rejected by the AVT");
                            return Response.Create(ResponseStatus.Error, new Message(r));
                        case 0x6: // avt filter
                            length = rx[0] & 0x0F;
                            status = false;
                            break;
                        case 0x8: // high speed notifications
                            length = rx[0] & 0x0F;
                            length--;
                            status = false;
                            break;
                        case 0x9: // init and version
                            length = rx[0] & 0x0F;
                            status = false;
                            break;
                        case 0xC: // C1 01 for 4x OK
                            length = rx[0] & 0x0F;
                            status = false;
                            break;
                        default:
                            this.Logger.AddDebugMessage("RX: Unhandled packet type " + type + ". Add support to ReadAVTPacket()");
                            status = false; // all non-zero high nibble type bytes have no status
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

            if (length <= 0) {
                this.Logger.AddDebugMessage("Not reading " + length + " byte packet");
                return Response.Create(ResponseStatus.Error, (Message)null);
            }

            // build a complete packet
            byte[] receive = new byte[length];
            byte[] packet = new byte[length];
            int bytes;
            DateTime start = DateTime.Now;
            DateTime stop = start + TimeSpan.FromSeconds(2);
            for (int i = 0; i < length; )
            {
                if (DateTime.Now > stop) return Response.Create(ResponseStatus.Timeout, (Message)null);
                bytes = await this.Port.Receive(receive, 0, length);
                Buffer.BlockCopy(receive, 0, packet, i, bytes);
                i += bytes;
            }
            
            //this.Logger.AddDebugMessage("Total Length=" + length + " RX: " + packet.ToHex());
            return Response.Create(ResponseStatus.Success, new Message(packet));
        }

        /// <summary>
        /// Convert a Message to an AVT formatted transmit, and send to the interface
        /// </summary>
        async private Task<Response<Message>> SendAVTPacket(Message message)
        {
            //this.Logger.AddDebugMessage("Trace: SendAVTPacket");

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

            //this.Logger.AddDebugMessage("send: " + message.GetBytes().ToHex());
            await this.Port.Send(message.GetBytes());
            
            return Response.Create(ResponseStatus.Success, message);
        }

        /// <summary>
        /// Configure AVT to return only packets targeted to the tool (Device ID F0), and disable transmit acks
        /// </summary>
        async private Task<Response<Boolean>> AVTSetup()
        {
            //this.Logger.AddDebugMessage("AVTSetup called");

            this.Logger.AddDebugMessage("Disable AVT Acks");
            await this.Port.Send(AVT_DISABLE_TX_ACK.GetBytes());
            Response<Message> m = await this.FindResponse(AVT_DISABLE_TX_ACK_OK);
            if (m.Status == ResponseStatus.Success)
            {
                this.Logger.AddDebugMessage("AVT Acks disabled");
            }
            else
            {
                this.Logger.AddUserMessage("Could not disable ACKs");
                this.Logger.AddDebugMessage("Expected " + AVT_DISABLE_TX_ACK_OK.ToString());
                return Response.Create(ResponseStatus.Error, false);
            }

            this.Logger.AddDebugMessage("Configure AVT filter");
            await this.Port.Send(AVT_FILTER_DEST.GetBytes());
            m = await this.FindResponse(AVT_FILTER_DEST_OK);
            if (m.Status == ResponseStatus.Success)
            {
                this.Logger.AddDebugMessage("AVT filter configured");
            }
            else
            {
                this.Logger.AddUserMessage("Could not configure AVT filter");
                this.Logger.AddDebugMessage("Expected " + AVT_FILTER_DEST_OK.ToString());
                return Response.Create(ResponseStatus.Error, false);
            }

            return Response.Create(ResponseStatus.Success, true);
        }

        /// <summary>
        /// Send a message, wait for a response, return the response.
        /// </summary>
        public override async Task<bool> SendMessage(Message message)
        {
            //this.Logger.AddDebugMessage("Sendrequest called");
            this.Logger.AddDebugMessage("TX: " + message.GetBytes().ToHex());
            await SendAVTPacket(message);
            return true;
        }

        protected async override Task Receive()
        {
           
            Response<Message> response = await ReadAVTPacket();
            if (response.Status == ResponseStatus.Success)
            {
                this.Logger.AddDebugMessage("RX: " + response.Value.GetBytes().ToHex());
                this.Enqueue(response.Value);
                return;
            }

            this.Logger.AddDebugMessage("AVT: no message waiting.");            
        }
        
        /// <summary>
        /// Set the interface to low (false) or high (true) speed
        /// </summary>
        /// <remarks>
        /// The caller must also tell the PCM to switch speeds
        /// </remarks>
        protected override async Task<bool> SetVpwSpeedInternal(VpwSpeed newSpeed)
        {

            if (newSpeed == VpwSpeed.Standard)
            {
                this.Logger.AddDebugMessage("AVT setting VPW 1X");
                await this.Port.Send(AvtDevice.AVT_1X_SPEED.GetBytes());
                await ReadAVTPacket(); // C1 00 (switched to 1x)
            }
            else
            {
                await ReadAVTPacket(); // 23 83 00 20 AVT generated response from generic PCM switch high speed command in Vehicle.cs
                this.Logger.AddDebugMessage("AVT setting VPW 4X");
                await this.Port.Send(AvtDevice.AVT_4X_SPEED.GetBytes());
                await ReadAVTPacket(); // C1 01 (switched to 4x)
            }

            return true;
        }

        public override void ClearMessageBuffer()
        {
            this.Port.DiscardBuffers();
            System.Threading.Thread.Sleep(50);
        }
    }    
}
