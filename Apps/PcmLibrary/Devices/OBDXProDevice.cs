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
    public class OBDXProDevice : SerialDevice
    {
        //Supported tools:
        //OBDX Pro VT v3- VPW only with USB, Wifi, BT, BLE
        //OBDX Pro VC - VPW only with USB
        //OBDX Pro GT - VPW with USB, Wifi, BT, BLE
        public string ToolConnected = "";
        public const string DeviceType = "OBDX Pro";
        public bool TimeStampsEnabled = false;
        public bool CRCInReceivedFrame = false;

        // This default is probably excessive but it should always be
        // overwritten by a call to SetTimeout before use anyhow.
        private VpwSpeed vpwSpeed = VpwSpeed.Standard;

        //ELM Command Set
        //To be put in, not really needed if using DVI command set

        //DVI (Direct Vehicle Interface) Command Set
        public static readonly Message DVI_BOARD_HARDWARE_VERSION = new Message(new byte[] { 0x22, 0x1, 0x0, 0 });
        public static readonly Message DVI_BOARD_FIRMWARE_VERSION = new Message(new byte[] { 0x22, 0x1, 0x1, 0 });
        public static readonly Message DVI_BOARD_MODEL = new Message(new byte[] { 0x22, 0x1, 0x2, 0 });
        public static readonly Message DVI_BOARD_NAME = new Message(new byte[] { 0x22, 0x1, 0x3, 0 });
        public static readonly Message DVI_UniqueSerial = new Message(new byte[] { 0x22, 0x1, 0x4, 0 });
        public static readonly Message DVI_Supported_OBD_Protocols = new Message(new byte[] { 0x22, 0x1, 0x5, 0 });
        public static readonly Message DVI_Supported_PC_Protocols = new Message(new byte[] { 0x22, 0x1, 0x6, 0 });

        public static readonly Message DVI_Req_NewtorkWriteStatus = new Message(new byte[] { 0x24, 0x1, 0x1, 0 });
        public static readonly Message DVI_Set_NewtorkWriteStatus = new Message(new byte[] { 0x24, 0x2, 0x1, 0, 0 });
        public static readonly Message DVI_Req_ConfigRespStatus = new Message(new byte[] { 0x24, 0x1, 0x2, 0 });
        public static readonly Message DVI_Set_CofigRespStatus = new Message(new byte[] { 0x24, 0x2, 0x2, 0, 0 });

        public static readonly Message DVI_Req_TimeStampOnRxNetwork = new Message(new byte[] { 0x24, 0x1, 0x3, 0 });
        public static readonly Message DVI_Set_TimeStampOnRxNetwork = new Message(new byte[] { 0x24, 0x2, 0x3, 0, 0 });

        public static readonly Message DVI_RESET = new Message(new byte[] { 0x25, 0x0, 0 });

        public static readonly Message DVI_Req_OBD_Protocol = new Message(new byte[] { 0x31, 0x1, 0x1, 0 });
        public static readonly Message DVI_Set_OBD_Protocol = new Message(new byte[] { 0x31, 0x2, 0x1, 0, 0 });
        public static readonly Message DVI_Req_NewtorkEnable = new Message(new byte[] { 0x31, 0x1, 0x2, 0 });
        public static readonly Message DVI_Set_NewtorkEnable = new Message(new byte[] { 0x31, 0x2, 0x2, 0, 0 });
        public static readonly Message DVI_Set_API_Protocol = new Message(new byte[] { 0x31, 0x2, 0x6, 0, 0 });

        public static readonly Message DVI_Req_To_Filter = new Message(new byte[] { 0x33, 0x1, 0x0, 0 });
        public static readonly Message DVI_Set_To_Filter = new Message(new byte[] { 0x33, 0x3, 0x0, 0, 0, 0 });
        public static readonly Message DVI_Req_From_Filter = new Message(new byte[] { 0x33, 0x1, 0x1, 0 });
        public static readonly Message DVI_Set_From_Filter = new Message(new byte[] { 0x33, 0x3, 0x1, 0, 0, 0 });
        public static readonly Message DVI_Req_RangeTo_Filter = new Message(new byte[] { 0x33, 0x1, 0x2, 0 });
        public static readonly Message DVI_Set_RangeTo_Filter = new Message(new byte[] { 0x33, 0x3, 0x2, 0, 0, 0, 0 });
        public static readonly Message DVI_Req_RangeFrom_Filter = new Message(new byte[] { 0x33, 0x1, 0x3, 0 });
        public static readonly Message DVI_Set_RangeFrom_Filter = new Message(new byte[] { 0x33, 0x4, 0x3, 0, 0, 0, 0 });
        public static readonly Message DVI_Req_Mask = new Message(new byte[] { 0x33, 0x1, 0x4, 0 });
        public static readonly Message DVI_Set_Mask = new Message(new byte[] { 0x33, 0x5, 0x4, 0, 0, 0, 0, 0 });
        public static readonly Message DVI_Req_Speed = new Message(new byte[] { 0x33, 0x1, 0x6, 0 });
        public static readonly Message DVI_Set_Speed = new Message(new byte[] { 0x33, 0x2, 0x6, 0, 0 });
        public static readonly Message DVI_Req_ValidateCRC_onRX = new Message(new byte[] { 0x33, 0x1, 0x7, 0 });
        public static readonly Message DVI_Set_ValidateCRC_onRX = new Message(new byte[] { 0x33, 0x2, 0x7, 0, 0 });
        public static readonly Message DVI_Req_Show_CRC_OnNetwork = new Message(new byte[] { 0x33, 0x1, 0x8, 0 });
        public static readonly Message DVI_Set_Show_CRC_OnNetwork = new Message(new byte[] { 0x33, 0x2, 0x8, 0, 0 });
        public static readonly Message DVI_Req_Write_Idle_Timeout = new Message(new byte[] { 0x33, 0x1, 0x9, 0 });
        public static readonly Message DVI_Set_Write_Idle_Timeout = new Message(new byte[] { 0x33, 0x3, 0x9, 0, 0, 0 });
        public static readonly Message DVI_Req_1x_Timings = new Message(new byte[] { 0x33, 0x2, 0xA, 0, 0 });
        public static readonly Message DVI_Set_1x_Timings = new Message(new byte[] { 0x33, 0x4, 0xA, 0, 0, 0, 0 });
        public static readonly Message DVI_Req_4x_Timings = new Message(new byte[] { 0x33, 0x2, 0xB, 0, 0 });
        public static readonly Message DVI_Set_4x_Timings = new Message(new byte[] { 0x33, 0x4, 0xB, 0, 0, 0, 0 });
        public static readonly Message DVI_Req_ResetTimings = new Message(new byte[] { 0x33, 0x2, 0xC, 0, 0 });
        public static readonly Message DVI_Req_ErrorBits = new Message(new byte[] { 0x33, 0x1, 0xD, 0 });
        public static readonly Message DVI_Set_ErrorBits = new Message(new byte[] { 0x33, 0x3, 0xD, 0, 0, 0 });
        public static readonly Message DVI_Req_ErrorCount = new Message(new byte[] { 0x33, 0x2, 0xE, 0, 0 });
        public static readonly Message DVI_Req_DefaultSettings = new Message(new byte[] { 0x33, 0x1, 0xF, 0 });

        public static readonly Message DVI_Req_ADC_SingleChannel = new Message(new byte[] { 0x35, 0x2, 0x0, 0, 0 });
        public static readonly Message DVI_Req_ADC_MultipleChannels = new Message(new byte[] { 0x35, 0x2, 0x1, 0, 0 });

        public OBDXProDevice(IPort port, ILogger logger) : base(port, logger)
        {
            this.MaxSendSize = 4096 + 10 + 2;    // packets up to 4112 but we want 4096 byte data blocks
            this.MaxReceiveSize = 4096 + 10 + 2; // with 10 byte header and 2 byte block checksum
            this.Supports4X = true;
            this.SupportsSingleDpidLogging = true;
            this.SupportsStreamLogging = true;

            // This will be used during device initialization.
            this.currentTimeoutScenario = TimeoutScenario.ReadProperty;
        }

        public override string GetDeviceType()
        {
            return DeviceType;
        }

        public override async Task<bool> Initialize()
        {
            this.Logger.AddDebugMessage("Initializing " + this.ToString());


            SerialPortConfiguration configuration = new SerialPortConfiguration();
            configuration.BaudRate = 115200;
            configuration.Timeout = 1000;
            await this.Port.OpenAsync(configuration);
            System.Threading.Thread.Sleep(200);
          // await Task.Delay(200);

            ////Reset scantool - ensures starts at ELM protocol
            bool Status = await ResetDevice();
            if (Status == false)
            {
                this.Logger.AddUserMessage("Unable to reset DVI device.");
                return false;
            }

             

            //Request Board information
            Response<string> BoardName = await GetBoardDetails();
            if (BoardName.Status != ResponseStatus.Success)
            {
                this.Logger.AddUserMessage("Unable to get DVI device details.");
                return false;
            }


            //Read voltage
            Response<double> ReadVoltageVal = await ReadVoltage();
            if (ReadVoltageVal.Status != ResponseStatus.Success)
            {
                this.Logger.AddUserMessage("Unable to read voltage.");
                return false;
            }
            this.Logger.AddUserMessage("Voltage is: " + ReadVoltageVal.Value.ToString("F2") + "V");


            //Set protocol to VPW mode
            Status = await SetProtocol(OBDProtocols.VPW);
            if (Status == false)
            {
                this.Logger.AddUserMessage("Unable to set DVI device protocol to VPW.");
                return false;
            }

            Response<bool> SetupStatus = await DVISetup();
            if (SetupStatus.Status != ResponseStatus.Success)
            {
                this.Logger.AddUserMessage("DVI device initialization failed.");
                return false;
            }

            this.Logger.AddUserMessage("Device Successfully Initialized and Ready");
            return true;
        }

        /// <summary>
        /// Not yet implemented.
        /// </summary>
        public override Task<TimeoutScenario> SetTimeout(TimeoutScenario scenario)
        {
            TimeoutScenario previousScenario = this.currentTimeoutScenario;
            this.currentTimeoutScenario = scenario;
            return Task.FromResult(previousScenario);
        }

        /// <summary>
        /// This will process incoming messages for up to 250ms looking for a message
        /// </summary>

        public async Task<Response<Message>> FindResponseFromTool(byte[] expected)
        {
            //this.Logger.AddDebugMessage("FindResponse called");
            for (int iterations = 0; iterations < 5; iterations++)
            {
                Response<Message> response = await this.ReadDVIPacket(this.GetReceiveTimeout());
                if (response != null)  // Hack to silence error - See: https://pcmhacking.net/forums/viewtopic.php?f=42&t=6730&start=110#p101790
                    if (response.Status == ResponseStatus.Success)
                        if (Utility.CompareArraysPart(response.Value.GetBytes(), expected))
                            return Response.Create(ResponseStatus.Success, (Message)response.Value);
                await Task.Delay(50);
            }

            return Response.Create(ResponseStatus.Timeout, (Message)null);
        }

        /// <summary>
        /// Wait for serial byte to be availble. False if timeout.
        /// </summary>
        async private Task<bool> WaitForSerial(ushort NumBytes, int timeout = 0)
        {
            if (timeout == 0)
            {
                timeout = 500;
            }

            int TempCount = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Wait for bytes to arrive...
            while (sw.ElapsedMilliseconds < timeout)
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
        /// Read an DVI formatted packet from the interface.
        /// If it recevies a Network message, in enqueues it and returns null;
        /// If it receives a Device message, it returns the message.
        /// </summary>
        async private Task<Response<Message>> ReadDVIPacket(int timeout = 0)
        {
            UInt16 Length = 0;

            byte offset = 0;
            byte[] rx = new byte[3]; // we dont read more than 3 bytes at a time
            byte[] timestampbuf = new byte[3];
            ulong timestampmicro = 0;
            // First Byte is command
            //Second is length, third also for long frame
            //Data
            //Checksum
            bool Chk = false;
            try
            {
                Chk = (await WaitForSerial(1, timeout));
                if (Chk == false)
                {
                    this.Logger.AddDebugMessage("Timeout.. no data present A");
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


            if (rx[0] == 0x8 || rx[0] == 0x9) //for network frames
            {
                //check if timestamps enabled
                if (TimeStampsEnabled)
                {
                    //next 4 bytes will be timestamp in microseconds
                    for (byte i = 0; i < 4; i++)
                    {
                        Chk = (await WaitForSerial(1));
                        if (Chk == false)
                        {
                            this.Logger.AddDebugMessage("Timeout.. no data present B");
                            return Response.Create(ResponseStatus.Timeout, (Message)null);
                        }
                        await this.Port.Receive(timestampbuf, i, 1);
                    }
                    timestampmicro = (ulong)((ulong)timestampbuf[0] * 0x100 ^ 3) + (ulong)((ulong)timestampbuf[1] * 0x100 ^ 2) + (ulong)((ulong)timestampbuf[0] * 0x100) + (ulong)timestampbuf[0];
                }
                if (rx[0] == 0x8) //if short, only get one byte for length
                {
                    Chk = (await WaitForSerial(1));
                    if (Chk == false)
                    {
                        this.Logger.AddDebugMessage("Timeout.. no data present C");
                        return Response.Create(ResponseStatus.Timeout, (Message)null);
                    }
                    await this.Port.Receive(rx, 1, 1);
                    Length = rx[1];
                }
                else //if long, get two bytes for length
                {
                    offset += 1;
                    Chk = (await WaitForSerial(2));
                    if (Chk == false)
                    {
                        this.Logger.AddDebugMessage("Timeout.. no data present D");
                        return Response.Create(ResponseStatus.Timeout, (Message)null);
                    }
                    await this.Port.Receive(rx, 1, 2);
                    Length = (ushort)((ushort)(rx[1] * 0x100) + rx[2]);
                }

            }
            else //for all other received frames
            {
                Chk = (await WaitForSerial(1));
                if (Chk == false)
                {
                    this.Logger.AddDebugMessage("Timeout.. no data present E");
                    return Response.Create(ResponseStatus.Timeout, (Message)null);
                }
                await this.Port.Receive(rx, 1, 1);
                Length = rx[1];
            }

            byte[] receive = new byte[Length + 3 + offset];
            Chk = (await WaitForSerial((ushort)(Length + 1)));
            if (Chk == false)
            {
                this.Logger.AddDebugMessage("Timeout.. no data present F");
                return Response.Create(ResponseStatus.Timeout, (Message)null);
            }

            int bytes;
            receive[0] = rx[0];//Command
            receive[1] = rx[1];//length
            if (rx[0] == 0x09) receive[2] = rx[2];//length long frame
            bytes = await this.Port.Receive(receive, 2 + offset, Length + 1);//get rest of frame
            if (bytes <= 0)
            {
                this.Logger.AddDebugMessage("Failed reading " + Length + " byte packet");
                return Response.Create(ResponseStatus.Error, (Message)null);
            }
            //should have entire frame now
            //verify checksum correct
            byte CalcChksm = 0;
            for (ushort i = 0; i < (receive.Length - 1); i++) CalcChksm += receive[i];
            if (rx[0] == 0x08 || rx[0] == 0x09)
            {
                if (TimeStampsEnabled)
                {
                    CalcChksm += timestampbuf[0];
                    CalcChksm += timestampbuf[1];
                    CalcChksm += timestampbuf[2];
                    CalcChksm += timestampbuf[3];
                }
            }
            CalcChksm = (byte)~CalcChksm;

            if (receive[receive.Length - 1] != CalcChksm)
            {
                this.Logger.AddDebugMessage("Total Length Data=" + Length + " RX: " + receive.ToHex());
                this.Logger.AddDebugMessage("Checksum error on received message.");
                return null;
            }

            // this.Logger.AddDebugMessage("Total Length Data=" + Length + " RX: " + receive.ToHex());

            if (receive[0] == 0x8 || receive[0] == 0x9)
            {
                //network frames //Strip header and checksum
                byte[] StrippedFrame = new byte[Length];
                Buffer.BlockCopy(receive, 2 + offset, StrippedFrame, 0, Length);
                this.Enqueue(new Message(StrippedFrame, timestampmicro, 0));

                // This can be useful for debugging, but is generally too noisy.
                 this.Logger.AddDebugMessage("Network RX: " + StrippedFrame.ToHex());
                return null;
            }
            else if (receive[0] == 0x7F)
            {
                // Error from the device
                Message result = new Message(receive);
                this.Logger.AddDebugMessage("XPro Error: " + result.ToString());
                return Response.Create(ResponseStatus.Error, result);
            }
            else
            {
                // Valid message from the device
                this.Logger.AddDebugMessage("XPro: " + receive.ToHex());
                return Response.Create(ResponseStatus.Success, new Message(receive));
            }
        }


        async private Task<Response<String>> ReadELMPacket(String SentFrame)
        {
            // UInt16 Counter = 0;
            bool framefound = false;
            bool Chk = false;

            string StrResp = "";
            byte[] rx = { 0 };
            try
            {
                while (framefound == false)
                {
                    Chk = (await WaitForSerial(1));
                    if (Chk == false)
                    {
                        this.Logger.AddDebugMessage("Timeout.. no data present");
                        return Response.Create(ResponseStatus.Timeout, "");
                    }

                    await this.Port.Receive(rx, 0, 1);
                    if (rx[0] == 0xD) //carriage return
                    {
                        if (StrResp != SentFrame)
                        {
                            framefound = true;
                            break;
                        }
                        StrResp = "";
                        continue;
                    }
                    else if (rx[0] == 0xA) continue;//newline
                    StrResp += Convert.ToChar(rx[0]);
                }

                //Find Idle frame
                framefound = false;
                while (framefound == false)
                {
                    Chk = (await WaitForSerial(1));
                    if (Chk == false)
                    {
                        this.Logger.AddDebugMessage("ELM Idle frame not detected");
                        return Response.Create(ResponseStatus.Timeout, "");
                    }
                    await this.Port.Receive(rx, 0, 1);
                    if (rx[0] == '>')
                    {
                        framefound = true;
                        break;
                    }
                }
                return Response.Create(ResponseStatus.Success, StrResp);

            }
            catch (Exception) // timeout exception - log no data, return error.
            {
                this.Logger.AddDebugMessage("No Data");
                return Response.Create(ResponseStatus.Timeout, (""));
            }

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

            // Wait for confirmation of successful send
            Response<Message> m = null;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                m = await ReadDVIPacket(500);
                if (m != null)
                {
                    if (m.Status == ResponseStatus.Timeout)
                    {
                        continue;
                    }
                    break;
                }
            }            

            if (m == null)
            {
                // This should never happen, but just in case...
                this.Logger.AddUserMessage("No response to send attempt. " + message.ToString());
                return Response.Create(ResponseStatus.Error, new Message(new byte[0]));
            }

            if (m.Status == ResponseStatus.Success)
            {
                byte[] Val = m.Value.GetBytes();
                if (Val[0] == 0x20 && Val[2] == 0x00)
                {
                    this.Logger.AddDebugMessage("TX: " + message.ToString());
                    return Response.Create(ResponseStatus.Success, message);
                }
                else if (Val[0] == 0x21 && Val[2] == 0x00)
                {
                    this.Logger.AddDebugMessage("TX: " + message.ToString());
                    return Response.Create(ResponseStatus.Success, message);
                }
                else
                {
                    this.Logger.AddUserMessage("Unable to transmit, odd response from device: " + message.ToString());
                    return Response.Create(ResponseStatus.Error, message);
                }
            }
            else
            {
                this.Logger.AddUserMessage("Unable to transmit, " + m.Status + ": " + message.ToString());
                return Response.Create(ResponseStatus.Error, message);
            }
        }

        /// <summary>
        /// Configure DVI to return only packets targeted to the tool (Device ID F0), and disable transmit acks
        /// </summary>
        async private Task<Response<Boolean>> DVISetup()
        {
            //Set filter
            bool Status = await SetToFilter(DeviceId.Tool);
            if (Status == false) return Response.Create(ResponseStatus.Error, false);

            //Enable network rx/tx for protocol
            Status = await EnableProtocolNetwork();
            if (Status == false) return Response.Create(ResponseStatus.Error, false);

            return Response.Create(ResponseStatus.Success, true);
        }

        async private Task<Response<double>> ReadVoltage()
        {
            byte[] Msg = new byte[] { 0x3A, 2, 0x0, (byte)0, 0 };
            Msg[Msg.Length - 1] = CalcChecksum(Msg);
            await this.Port.Send(Msg);

            byte[] RespBytes = new byte[Msg.Length];
            Array.Copy(Msg, RespBytes, Msg.Length);
            RespBytes[0] += (byte)0x10;
            RespBytes[RespBytes.Length - 1] = CalcChecksum(RespBytes);
            Response<Message> response = await ReadDVIPacket();
            if (response.Status != ResponseStatus.Success)
            {
             //   this.Logger.AddDebugMessage("Network enabled");
                return Response.Create(response.Status, (double)0);
            }
            else
            {
                int RawADC = (int)((response.Value[4] * Math.Pow(0x100, 1)) + response.Value[5]);
                double COnvertedVoltage = ((((double)RawADC * 0.009047468) + 0.2)); //Should match for both VT and GT (Close enough).
                //this.Logger.AddDebugMessage("Voltage is: " + COnvertedVoltage.ToString("F2") + "V"); //2 decimal places
                return Response.Create(ResponseStatus.Success, COnvertedVoltage);
            }
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

        /// <summary>
        /// Receive a message from the network - or at least try to.
        /// </summary>
        /// <remarks>
        /// Messages are placed into the queue by the code in ReadDvIPacket.
        /// Retry loops and message processing are in the application layer.
        /// </remarks>
        protected async override Task Receive()
        {
            await ReadDVIPacket();
        }

        private async Task<bool> ResetDevice()
        {
            //Send DVI reset
            byte[] Msg = OBDXProDevice.DVI_RESET.GetBytes();
            Msg[Msg.Length - 1] = CalcChecksum(Msg);
            await this.Port.Send(Msg);
            System.Threading.Thread.Sleep(200);
           // await Task.Delay(200);
            await this.Port.DiscardBuffers();

            //Send ELM reset
            byte[] MsgATZ = { (byte)'A', (byte)'T', (byte)'Z', 0xD };
            await this.Port.Send(MsgATZ);
            System.Threading.Thread.Sleep(50);
            await this.Port.Send(MsgATZ);
            System.Threading.Thread.Sleep(400);
            await this.Port.DiscardBuffers();


            //AT@1 will return OBDX Pro VT - will then need to change its API to DVI bytes.
            byte[] MsgAT1 = { (byte)'A', (byte)'T', (byte)'@', (byte)'1', 0xD };
            await this.Port.Send(MsgAT1);
            Response<String> m = await ReadELMPacket("AT@1");
            if (m.Status == ResponseStatus.Success) this.Logger.AddUserMessage("Device Found: " + m.Value);
            else { this.Logger.AddUserMessage("OBDX Pro device not found or failed response"); return false; }

            System.Threading.Thread.Sleep(150);
            await this.Port.DiscardBuffers();

            //Change to DVI protocol DX 
            byte[] MsgDXDP = { (byte)'D', (byte)'X', (byte)'D', (byte)'P', (byte)'1', 0xD };
            await this.Port.Send(MsgDXDP);
            m = await ReadELMPacket("DXDP1");
            if (m.Status == ResponseStatus.Success && m.Value == "OK") this.Logger.AddDebugMessage("Switched to DVI protocol");
            else { this.Logger.AddUserMessage("Failed to switch to DVI protocol"); return false; }
            return true;
        }

        private async Task<Response<string>> GetBoardDetails()
        {
            string Details = "";
            byte[] Msg = OBDXProDevice.DVI_BOARD_NAME.GetBytes();
            Msg[Msg.Length - 1] = CalcChecksum(Msg);
            await this.Port.Send(Msg);

            Response<Message> m = await ReadDVIPacket();
            if (m.Status == ResponseStatus.Success)
            {
                byte[] Val = m.Value.GetBytes();
                ToolConnected = System.Text.Encoding.ASCII.GetString(Val, 3, Val[1] - 1);
                //  this.Logger.AddUserMessage("Device Found: " + name);
                // return new Response<String>(ResponseStatus.Success, name);
            }
            else
            {
                this.Logger.AddUserMessage("OBDX Pro device not found or failed response");
                return new Response<String>(ResponseStatus.Error, null);
            }
            Details = ToolConnected;

            if (ToolConnected == "OBDX Pro VC") //Must reduce block size
            {
                this.MaxSendSize = 2048 + 10 + 2;    // 2048 byte data blocks with 10 byte 
                this.MaxReceiveSize = 2048 + 10 + 2; // header and 2 byte block checksum
            }

            //Firmware version
            Msg = OBDXProDevice.DVI_BOARD_FIRMWARE_VERSION.GetBytes();
            Msg[Msg.Length - 1] = CalcChecksum(Msg);
            await this.Port.Send(Msg);
            m = await ReadDVIPacket();
            if (m.Status == ResponseStatus.Success)
            {
                byte[] Val = m.Value.GetBytes();
                string Firmware = "";
                if (ToolConnected == "OBDX Pro VT")
                {
                    Firmware = ((float)(Val[3] * 0x100 + Val[4]) / 100).ToString("n2");
                }
                else //new firmware standard
                {
                    Firmware = Val[3].ToString() + "." + Val[4].ToString() + "." + Val[5].ToString() + "." + Val[6].ToString();
                }
               
                this.Logger.AddDebugMessage("Firmware version: v" + Firmware);
                Details += " - Firmware: v" + Firmware;
            }
            else
            {
                this.Logger.AddUserMessage("Unable to read firmware version");
                return new Response<String>(ResponseStatus.Error, null);
            }

            //Hardware version
            Msg = OBDXProDevice.DVI_BOARD_HARDWARE_VERSION.GetBytes();
            Msg[Msg.Length - 1] = CalcChecksum(Msg);
            await this.Port.Send(Msg);
            m = await ReadDVIPacket();
            if (m.Status == ResponseStatus.Success)
            {
                byte[] Val = m.Value.GetBytes();
                string Hardware = "";
                if (ToolConnected == "OBDX Pro VT")
                {
                    Hardware = ((float)(Val[3] * 0x100 + Val[4]) / 100).ToString("n2");
                }
                else //new firmware standard
                {
                    Hardware = Val[3].ToString() + "." + Val[4].ToString() + "." + Val[5].ToString() + "." + Val[6].ToString();
                }
                 
                this.Logger.AddDebugMessage("Hardware version: v" + Hardware);
                Details += " - Hardware: v" + Hardware;
            }
            else
            {
                this.Logger.AddUserMessage("Unable to read hardware version");
                return new Response<String>(ResponseStatus.Error, null);
            }


            //Unique Serial
            Msg = OBDXProDevice.DVI_UniqueSerial.GetBytes();
            Msg[Msg.Length - 1] = CalcChecksum(Msg);
            await this.Port.Send(Msg);
            m = await ReadDVIPacket();
            if (m.Status == ResponseStatus.Success)
            {
                byte[] Val = m.Value.GetBytes();
                byte[] serial = new byte[12];
                Array.Copy(Val, 3, serial, 0, 12);
                String Serial = string.Join("", Array.ConvertAll(serial, b => b.ToString("X2")));
                this.Logger.AddDebugMessage("Unique Serial: " + Serial);
                Details += " - Unique Serial: " + Serial;
                return new Response<String>(ResponseStatus.Success, Details);
            }
            else
            {
                this.Logger.AddUserMessage("Unable to read unique Serial");
                return new Response<String>(ResponseStatus.Error, null);
            }
        }

        enum OBDProtocols : UInt16
        {
            VPW = 1
        }
        private async Task<bool> SetProtocol(OBDProtocols val)
        {
            byte[] Msg = OBDXProDevice.DVI_Set_OBD_Protocol.GetBytes();
            Msg[3] = (byte)val;
            Msg[Msg.Length - 1] = CalcChecksum(Msg);
            await this.Port.Send(Msg);

            //get response
            byte[] RespBytes = new byte[Msg.Length];
            Array.Copy(Msg, RespBytes, Msg.Length);
            RespBytes[0] += (byte)0x10;
            RespBytes[RespBytes.Length - 1] = CalcChecksum(RespBytes);
            Response<Message> m = await FindResponseFromTool(RespBytes);
            if (m.Status == ResponseStatus.Success)
            {
                this.Logger.AddDebugMessage("OBD Protocol Set to VPW");
            }
            else
            {
                this.Logger.AddUserMessage("Unable to set OBDX Pro device to VPW mode");
                this.Logger.AddDebugMessage("Expected " + string.Join(" ", Array.ConvertAll(Msg, b => b.ToString("X2"))));
                return false;
            }
            return true;
        }

        private async Task<bool> SetToFilter(byte Val)
        {
            byte[] Msg = OBDXProDevice.DVI_Set_To_Filter.GetBytes();
            Msg[3] = Val; // DeviceId.Tool;
            Msg[4] = 1; //on
            Msg[Msg.Length - 1] = CalcChecksum(Msg);
            await this.Port.Send(Msg);

            byte[] RespBytes = new byte[Msg.Length];
            Array.Copy(Msg, RespBytes, Msg.Length);
            RespBytes[0] += (byte)0x10;
            RespBytes[RespBytes.Length - 1] = CalcChecksum(RespBytes);
            Response<Message> response = await ReadDVIPacket();
            if (response.Status == ResponseStatus.Success & Utility.CompareArraysPart(response.Value.GetBytes(), RespBytes))
            {
                this.Logger.AddDebugMessage("Filter set and enabled");
                return true;
            }
            else
            {

                this.Logger.AddDebugMessage("Failed to set filter");
                return false;
            }
        }

        private async Task<bool> EnableProtocolNetwork()
        {
            byte[] Msg = OBDXProDevice.DVI_Set_NewtorkEnable.GetBytes();
            Msg[3] = 1; //on
            Msg[Msg.Length - 1] = CalcChecksum(Msg);
            await this.Port.Send(Msg);

            byte[] RespBytes = new byte[Msg.Length];
            Array.Copy(Msg, RespBytes, Msg.Length);
            RespBytes[0] += (byte)0x10;
            RespBytes[RespBytes.Length - 1] = CalcChecksum(RespBytes);
            Response<Message> response = await ReadDVIPacket();
            if (response.Status == ResponseStatus.Success & Utility.CompareArraysPart(response.Value.GetBytes(), RespBytes))
            {
                this.Logger.AddDebugMessage("Network enabled");
                return true;
            }
            else
            {
                this.Logger.AddDebugMessage("Failed to enable network");
                return false;
            }
        }

        /// <summary>
        /// Set the interface to 1x or 4x speed
        /// </summary>
        /// <remarks>
        /// The caller must also tell the PCM to switch speeds
        /// </remarks>
        protected override async Task<bool> SetVpwSpeedInternal(VpwSpeed newSpeed)
        {

            byte[] Msg = OBDXProDevice.DVI_Set_Speed.GetBytes();

            if (newSpeed == VpwSpeed.Standard)
            {
                this.Logger.AddDebugMessage("DVI setting VPW 1X");
                Msg[3] = 0;
                this.vpwSpeed = VpwSpeed.Standard;

            }
            else
            {
                this.Logger.AddDebugMessage("DVI setting VPW 4X");
                Msg[3] = 1;
                this.vpwSpeed = VpwSpeed.FourX;
            }

            Msg[Msg.Length - 1] = CalcChecksum(Msg);
            await this.Port.Send(Msg);

            byte[] RespBytes = new byte[Msg.Length];
            Array.Copy(Msg, RespBytes, Msg.Length);
            RespBytes[0] += (byte)0x10;
            RespBytes[RespBytes.Length - 1] = CalcChecksum(RespBytes);
            Response<Message> m = await FindResponseFromTool(RespBytes);
            if (m.Status != ResponseStatus.Success) return false;

            return true;
        }


        enum ConfigurationErrors : byte
        {
            InvalidCommand = 1,
            RecvTooLong = 2,
            ByteWaitTimeout = 3,
            InvalidSerialChksum = 4,
            SubCommandIncorrectSize = 5,
            InvalidSubCommand = 6,
            SubCommandInvalidData = 7
        }
        enum NetworkErrors : byte
        {
            ReadBusInactive = 0,
            ReadSOFLongerThenMax = 1,
            ReadSOFShorterThenMin = 2,
            Read2BytesOrLess = 3,
            ReadCRCIncorrect = 4,
            ReadFilterTOdoesNotMatch = 5,
            ReadFilterFROMdoesNotMatch = 6,
            ReadRangeFilterTOdoesNotMatch = 7,
            ReadRangeFilterFROMdoesNotMatch = 8,
            WriteFrameIdleFindTimeout = 9,
            NotEnabled = 10,
            ReadOnlyMode = 11,
            ReadFrameAwaitingSendPC = 12
        }
        enum ErrorType : byte
        {
            ConfigOrTxNetwork = 0,
            RxNetwork = 1
        }


        private void ProcessError(ErrorType Type, byte code)
        {
            string ErrVal = "";
            if (Type == ErrorType.ConfigOrTxNetwork)
            {
                switch (code)
                {
                    case (byte)ConfigurationErrors.InvalidCommand:
                        ErrVal = "Invalid command byte received";
                        break;
                    case (byte)ConfigurationErrors.RecvTooLong:
                        ErrVal = "Sent frame is larger then max allowed frame (4200)";
                        break;
                    case (byte)ConfigurationErrors.ByteWaitTimeout:
                        ErrVal = "Timeout occured waiting for byte to be received from PC";
                        break;
                    case (byte)ConfigurationErrors.InvalidSerialChksum:
                        ErrVal = "Invalid frame checksum sent to scantool";
                        break;
                    case (byte)ConfigurationErrors.SubCommandIncorrectSize:
                        ErrVal = "Sent command had incorrect length";
                        break;
                    case (byte)ConfigurationErrors.InvalidSubCommand:
                        ErrVal = "Invalid sub command detected";
                        break;
                    case (byte)ConfigurationErrors.SubCommandInvalidData:
                        ErrVal = "Invalid data detected for sub command";
                        break;
                }
            }
            else if (Type == ErrorType.RxNetwork)
            {

            }
            this.Logger.AddDebugMessage("Fault reported from scantool: " + ErrVal);
        }

        public override void ClearMessageBuffer()
        {
            this.Port.DiscardBuffers();
        }

        /// <summary>
        /// This is based on the timeouts used by the AllPro, so it could probably be optimized further.
        /// </summary>
        private int GetReceiveTimeout()
        {
            int result;
            if (this.vpwSpeed == VpwSpeed.Standard)
            {
                switch (this.currentTimeoutScenario)
                {
                    case TimeoutScenario.Minimum:
                        result = 50;
                        break;

                    case TimeoutScenario.ReadProperty:
                        result = 50;
                        break;

                    case TimeoutScenario.ReadCrc:
                        result = 250;
                        break;

                    case TimeoutScenario.ReadMemoryBlock:
                        result = 250;
                        break;

                    case TimeoutScenario.EraseMemoryBlock:
                        result = 7000;
                        break;

                    case TimeoutScenario.WriteMemoryBlock:
                        result = 1200;
                        break;

                    case TimeoutScenario.SendKernel:
                        result = 4000;
                        break;

                    case TimeoutScenario.DataLogging1:
                        result = 25;
                        break;

                    case TimeoutScenario.DataLogging2:
                        result = 40;
                        break;

                    case TimeoutScenario.DataLogging3:
                        result = 60;
                        break;

                    case TimeoutScenario.DataLogging4:
                        result = 80;
                        break;

                    case TimeoutScenario.DataLoggingStreaming:
                        result = 0;
                        break;

                    case TimeoutScenario.Maximum:
                        result = 1020;
                        break;

                    default:
                        throw new NotImplementedException("Unknown timeout scenario " + this.currentTimeoutScenario);
                }
            }
            else
            {
                switch (this.currentTimeoutScenario)
                {
                    case TimeoutScenario.Minimum:
                        result = 50;
                        break;

                    case TimeoutScenario.ReadProperty:
                        result = 50;
                        break;

                    case TimeoutScenario.ReadCrc:
                        result = 250;
                        break;

                    case TimeoutScenario.ReadMemoryBlock:
                        result = 250;
                        break;

                    case TimeoutScenario.EraseMemoryBlock:
                        result = 7000;
                        break;

                    case TimeoutScenario.WriteMemoryBlock:
                        result = 600;
                        break;

                    case TimeoutScenario.SendKernel:
                        result = 2000;
                        break;

                    case TimeoutScenario.DataLogging1:
                        result = 7;
                        break;

                    case TimeoutScenario.DataLogging2:
                        result = 10;
                        break;

                    case TimeoutScenario.DataLogging3:
                        result = 15;
                        break;

                    case TimeoutScenario.Maximum:
                        result = 1020;
                        break;

                    default:
                        throw new NotImplementedException("Unknown timeout scenario " + this.currentTimeoutScenario);
                }
            }

            return result;
        }
    }
}
