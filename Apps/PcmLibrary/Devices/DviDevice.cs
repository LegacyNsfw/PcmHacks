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
    /// This class encapsulates all code that is unique to the DVI interface.
    /// </summary>
    /// 
    public class DviDevice : SerialDevice
    {
        public const string DeviceType = "DVI";

        public static readonly Message DVI_RESET = new Message(new byte[] { 0x25, 0x0, 0xDA });
        public static readonly Message DVI_BOARD_FIRMWARE = new Message(new byte[] { 0x22, 0x1, 0x1, 0xDB });
        public static readonly Message DVI_BOARD_MODEL = new Message(new byte[] { 0x22, 0x1, 0x2, 0xDA });
        public static readonly Message DVI_BOARD_NAME = new Message(new byte[] { 0x22, 0x1, 0x3, 0xD9 });
        public static readonly Message DVI_SUPPORTED_PROTOCOLS = new Message(new byte[] { 0x22, 0x1, 0x5, 0xD8 });
        public static readonly Message DVI_ENTER_VPW_MODE = new Message(new byte[] { 0x31, 0x02, 0x01, 0x01, 0xCA });

        public static readonly Message DVI_1X_SPEED = new Message(new byte[] { 0x33, 0x02, 06, 00, 0Xc4 });
        public static readonly Message DVI_4X_SPEED = new Message(new byte[] { 0x33, 0x02, 06, 01, 0Xc3 });

        // DVI reader responses
        public static readonly Message DVI_RESET_RESP = new Message(new byte[] { 0x35, 0x0, 0xCA });
        public static readonly Message DVI_VPW_RESP = new Message(new byte[] { 0x41, 02, 01, 01, 0xBA });
        public static readonly Message DVI_VPW1X_RESP = new Message(new byte[] { 0x43, 0x02, 06, 0, 0XB4 });
        public static readonly Message DVI_VPW4X_RESP = new Message(new byte[] { 0x43, 0x02, 06, 1, 0XB3 });


        public DviDevice(IPort port, ILogger logger) : base(port, logger)
        {
            this.MaxSendSize = 4096 + 10 + 2;    // packets up to 4112 but we want 4096 byte data blocks
            this.MaxReceiveSize = 4096 + 10 + 2; // with 10 byte header and 2 byte block checksum
            this.Supports4X = true;
        }

        public override string GetDeviceType()
        {
            return DeviceType;
        }

        public override async Task<bool> Initialize()
        {
            this.Logger.AddDebugMessage("Initializing " + this.ToString());


            SerialPortConfiguration configuration = new SerialPortConfiguration();
            configuration.BaudRate = 500000;
            await this.Port.OpenAsync(configuration);
            System.Threading.Thread.Sleep(100);
            await this.Port.DiscardBuffers();

            ////Reset scantool first
            //this.Logger.AddDebugMessage("Sending 'reset' message.");
            //await this.Port.Send(DVIDevice.DVI_RESET.GetBytes());
            //Response<Message> m = await this.FindResponse(DVI_RESET_RESP);
            //this.Logger.AddUserMessage("DVI Reset OK");

            //
            //  System.Threading.Thread.Sleep(1000);

            //Request Board information
            await this.Port.Send(DviDevice.DVI_BOARD_NAME.GetBytes());
            Response<Message> m = await ReadDVIPacket();
            if (m.Status == ResponseStatus.Success)
            {
                byte[] Val = m.Value.GetBytes();
                string name = System.Text.Encoding.ASCII.GetString(Val, 3, Val[1] - 1);
                this.Logger.AddUserMessage("Device Found: " + name);
            }
            else
            {
                this.Logger.AddUserMessage("DVI device not found or failed response");
                return false;
            }

            await this.Port.DiscardBuffers();

            //Set protocol to VPW mode
            await this.Port.Send(DviDevice.DVI_ENTER_VPW_MODE.GetBytes());
            m = await FindResponse(DVI_VPW_RESP);
            if (m.Status == ResponseStatus.Success)
            {
                this.Logger.AddDebugMessage("Set VPW Mode");
            }
            else
            {
                this.Logger.AddUserMessage("Unable to set DVI device to VPW mode");
                this.Logger.AddDebugMessage("Expected " + DviDevice.DVI_VPW_RESP.ToString());
                return false;
            }

            await DVISetup();

            return true;
        }

        /// <summary>
        /// Not yet implemented.
        /// </summary>
        public override Task SetTimeout(TimeoutScenario scenario)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// This will process incoming messages for up to 500ms looking for a message
        /// </summary>
        public async Task<Response<Message>> FindResponse(Message expected)
        {
            //this.Logger.AddDebugMessage("FindResponse called");
            for (int iterations = 0; iterations < 3; iterations++)
            {
                Response<Message> response = await this.ReadDVIPacket();
                if (response.Status == ResponseStatus.Success)
                    if (Utility.CompareArraysPart(response.Value.GetBytes(), expected.GetBytes()))
                        return Response.Create(ResponseStatus.Success, (Message)response.Value);
                await Task.Delay(100);
            }

            return Response.Create(ResponseStatus.Timeout, (Message)null);
        }

        /// <summary>
        /// Wait for serial byte to be availble. False if timeout.
        /// </summary>
        async private Task<bool> WaitForSerial(ushort NumBytes)
        {
            int TempCount = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < 500) //wait for byte...
            {
                if (await this.Port.GetReceiveQueueSize() > TempCount)
                {
                    TempCount = await this.Port.GetReceiveQueueSize();
                    sw.Restart();
                }
                if (await this.Port.GetReceiveQueueSize() >= NumBytes) { return true; }
            }
            return false;
        }

        /// <summary>
        /// Read an DVI formatted packet from the interface, and return a Response/Message
        /// </summary>
        async private Task<Response<Message>> ReadDVIPacket()
        {

            //this.Logger.AddDebugMessage("Trace: ReadDVIPacket");
            UInt16 Length = 0;

            byte offset = 0;
            byte[] rx = new byte[3]; // we dont read more than 3 bytes at a time


            // First Byte is command
            //Second third (possibly fourth if long frame)
            //Data
            //Checksum
            bool Chk = false;
            try
            {
                Chk = (await WaitForSerial(1));
                if (Chk == false)
                {
                    this.Logger.AddDebugMessage("Timeout.. no data present");
                    return Response.Create(ResponseStatus.Timeout, (Message)null);
                }

                //get first byte for command
                await this.Port.Receive(rx, 0, 1);
            }
            catch (Exception) // timeout exception - log no data, return error.
            {
                this.Logger.AddDebugMessage("No Data");
                return Response.Create(ResponseStatus.Timeout, (Message)null);
            }

            // read an DVI format length
            switch (rx[0])
            {
                case 0x09: //long network frame
                    offset = 1;
                    Chk = (await WaitForSerial(2));
                    if (Chk == false)
                    {
                        this.Logger.AddDebugMessage("Timeout.. no data present");
                        return Response.Create(ResponseStatus.Timeout, (Message)null);
                    }

                    await this.Port.Receive(rx, 1, 2);
                    Length = (ushort)((ushort)(rx[1] * 0x100) + rx[2]);
                    break;
                default: //else all 1 byte lengths   
                    Chk = (await WaitForSerial(1));
                    if (Chk == false)
                    {
                        this.Logger.AddDebugMessage("Timeout.. no data present");
                        return Response.Create(ResponseStatus.Timeout, (Message)null);
                    }

                    await this.Port.Receive(rx, 1, 1);
                    Length = rx[1];
                    break;
            }

            byte[] receive = new byte[Length + 3 + offset];
            Chk = (await WaitForSerial((ushort)(Length + 1)));
            if (Chk == false)
            {
                this.Logger.AddDebugMessage("Timeout.. no data present");
                return Response.Create(ResponseStatus.Timeout, (Message)null);
            }

            int bytes;
            receive[0] = rx[0];
            receive[1] = rx[1];
            if (rx[0] == 0x09) receive[2] = rx[2];
            bytes = await this.Port.Receive(receive, 2 + offset, Length + 1);
            if (bytes <= 0)
            {
                this.Logger.AddDebugMessage("Failed reading " + Length + " byte packet");
                return Response.Create(ResponseStatus.Error, (Message)null);
            }
            //should have entire frame now
            //verify checksum correct
            byte CalcChksm = 0;
            for (ushort i = 0; i < (receive.Length - 1); i++) CalcChksm += receive[i];
            CalcChksm = (byte)~CalcChksm;

            if (receive[receive.Length - 1] != CalcChksm)
            {
                this.Logger.AddDebugMessage("Checksum error on received message.");
                return Response.Create(ResponseStatus.Error, (Message)null);
            }

            this.Logger.AddDebugMessage("Total Length Data=" + Length + " RX: " + receive.ToHex());


            if (receive[0] == 0x8 || receive[0] == 0x9)
            {//network frames //Strip header and checksum
                byte[] StrippedFrame = new byte[Length];
                Buffer.BlockCopy(receive, 2 + offset, StrippedFrame, 0, Length);
                return Response.Create(ResponseStatus.Success, new Message(StrippedFrame));
            }
            else return Response.Create(ResponseStatus.Success, new Message(receive));
        }


        /// <summary>
        /// Calc checksum for byte array for all messages to/from device
        /// </summary>
        private byte CalcChecksum(byte[] MyArr)
        {
            byte CalcChksm = 0;
            for (ushort i = 0; i < (MyArr.Length - 1); i++)
            {
                CalcChksm += MyArr[i];
            }
            return ((byte)~CalcChksm);
        }


        /// <summary>
        /// Convert a Message to an DVI formatted transmit, and send to the interface
        /// </summary>
        async private Task<Response<Message>> SendDVIPacket(Message message)
        {
            //this.Logger.AddDebugMessage("Trace: SendDVIPacket");
            int length = message.GetBytes().Length;
            byte[] RawPacket = message.GetBytes();
            byte[] SendPacket = new byte[length + 3];

            if (length > 0xFF)
            {
                System.Array.Resize(ref SendPacket, SendPacket.Length + 1);
                SendPacket[0] = 0x11;
                SendPacket[1] = (byte)(length >> 8);
                SendPacket[2] = (byte)length;
                Buffer.BlockCopy(RawPacket, 0, SendPacket, 3, length);
            }
            else
            {
                SendPacket[0] = 0x10;
                SendPacket[1] = (byte)length;
                Buffer.BlockCopy(RawPacket, 0, SendPacket, 2, length);
            }

            //Add checksum
            SendPacket[SendPacket.Length - 1] = CalcChecksum(SendPacket);

            //Send frame
            await this.Port.Send(SendPacket);
            this.Logger.AddDebugMessage("send: " + SendPacket.ToHex());

            //  return Response.Create(ResponseStatus.Success, message);

            //check sent successfully
            Response<Message> m = await ReadDVIPacket();
            if (m.Status == ResponseStatus.Success)
            {
                byte[] Val = m.Value.GetBytes();
                if (Val[0] == 0x20 && Val[2] == 0x01)
                {
                    return Response.Create(ResponseStatus.Success, message);
                }
                return Response.Create(ResponseStatus.Error, message);
            }
            else
            {
                return Response.Create(ResponseStatus.Error, message);
            }
            //  return Response.Create(ResponseStatus.Error, message);

        }

        /// <summary>
        /// Configure DVI to return only packets targeted to the tool (Device ID F0), and disable transmit acks
        /// </summary>
        async private Task<Response<Boolean>> DVISetup()
        {
            //this.Logger.AddDebugMessage("DVISetup called");
            byte[] filterdest = { 0x33, 0x03, 0, DeviceId.Tool, 1, 0 }; filterdest[filterdest.Length - 1] = CalcChecksum(filterdest);
            byte[] filterdestok = { 0x43, 0x03, 0, DeviceId.Tool, 1, 0 }; filterdestok[filterdestok.Length - 1] = CalcChecksum(filterdestok);
            byte[] enableProtocol = { 0x31, 0x02, 0x05, 1, 0 }; enableProtocol[enableProtocol.Length - 1] = CalcChecksum(enableProtocol);///0=off,1=on,2=readonly
            byte[] enableProtocolok = { 0x41, 0x02, 0x05, 1, 0 }; enableProtocolok[enableProtocolok.Length - 1] = CalcChecksum(enableProtocolok);

            //Send filter enable
            await this.Port.Send(filterdest);
            Response<Message> response = await ReadDVIPacket();

            if (response.Status == ResponseStatus.Success & Utility.CompareArraysPart(response.Value.GetBytes(), filterdestok))
            {
                this.Logger.AddDebugMessage("DVI filter set and enabled");
            }
            else
            {
                this.Logger.AddDebugMessage("Failed to filter");
                return Response.Create(ResponseStatus.Error, false);
            }



            //Send protocol enable
            await this.Port.Send(enableProtocol);
            response = await ReadDVIPacket();

            if (response.Status == ResponseStatus.Success & Utility.CompareArraysPart(response.Value.GetBytes(), enableProtocolok))
            {
                this.Logger.AddDebugMessage("Protocol enabled");
            }
            else
            {
                this.Logger.AddDebugMessage("Failed to enable protocol");
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
            //  this.Logger.AddDebugMessage("TX: " + message.GetBytes().ToHex());
            await SendDVIPacket(message);




            return true;
        }

        protected async override Task Receive()
        {

            Response<Message> response = await ReadDVIPacket();
            if (response.Status == ResponseStatus.Success)
            {
                this.Logger.AddDebugMessage("RX: " + response.Value.GetBytes().ToHex());
                this.Enqueue(response.Value);
                return;
            }

            this.Logger.AddDebugMessage("DVI: no message waiting.");
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
                this.Logger.AddDebugMessage("DVI setting VPW 1X");
                await this.Port.Send(DviDevice.DVI_1X_SPEED.GetBytes());
                Response<Message> m = await this.FindResponse(DVI_VPW1X_RESP);
                if (m.Status != ResponseStatus.Success)
                {
                    return false;
                }

            }
            else
            {
                this.Logger.AddDebugMessage("DVI setting VPW 4X");
                await this.Port.Send(DviDevice.DVI_4X_SPEED.GetBytes());
                Response<Message> m = await this.FindResponse(DVI_VPW4X_RESP);
                if (m.Status != ResponseStatus.Success)
                {
                    return false;
                }
            }

            return true;
        }

        public override void ClearMessageBuffer()
        {
            this.Port.DiscardBuffers();

            //  System.Threading.Thread.Sleep(50);
        }
    }

}
