using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J2534;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace PcmHacking
{
    /// <summary>
    /// This class encapsulates all code that is unique to the AVT 852 interface.
    /// </summary>
    ///
    class J2534Device : Device
    {
        /// <summary>
        /// Configuration settings
        /// </summary>
        public int ReadTimeout = 2000;
        public int WriteTimeout = 2000;

        /// <summary>
        /// variety of properties used to id channels, fitlers and status
        /// </summary>
        private J2534_Struct J2534Port;
        public List<ulong> Filters;
        private uint DeviceID;
        private uint ChannelID;
        private ProtocolID Protocol;
        public bool IsProtocolOpen;
        public bool IsJ2534Open;
        private const string PortName = "J2534";
        private const uint MessageFilter = 0x6CF010;
        public string ToolName = "";

        /// <summary>
        /// global error variable for reading/writing. (Could be done on the fly)
        /// TODO, keep record of all errors for debug
        /// </summary>
        public J2534Err OBDError;

        /// <summary>
        /// J2534 has two parts.
        /// J2534device which has the supported protocols ect as indicated by dll and registry.
        /// J2534extended which is al the actual commands and functions to be used. 
        /// </summary>
        struct J2534_Struct
        {
            public J2534Extended Functions;
            public J2534.J2534Device LoadedDevice;
        }

        public J2534Device(J2534.J2534Device jport, ILogger logger) : base(logger)
        {
            J2534Port = new J2534_Struct();
            J2534Port.Functions = new J2534Extended();
            J2534Port.LoadedDevice = jport;

            // Reduced from 4096+12 for the MDI2
            this.MaxSendSize = 2048 + 12;    // J2534 Standard is 4KB
            this.MaxReceiveSize = 2048 + 12; // J2534 Standard is 4KB
            this.Supports4X = true;
            this.SupportsSingleDpidLogging = true;
            this.SupportsStreamLogging = true;
        }

        protected override void Dispose(bool disposing)
        {
            DisconnectTool();
        }

        public override string ToString()
        {
            return "J2534 Device";
        }

        // This needs to return Task<bool> for consistency with the Device base class.
        // However it doesn't do anything asynchronous, so to make the code more readable
        // it just wraps a private method that does the real work and returns a bool.
        public override async Task<bool> Initialize()
        {
            return await Task.FromResult(this.InitializeInternal());
        }

        // This returns 'bool' for the sake of readability. That bool needs to be
        // wrapped in a Task object for the public Initialize method.
        private bool InitializeInternal()
        {
            Filters = new List<ulong>();

            this.Logger.AddUserMessage("Initializing " + this.ToString());

            Response<J2534Err> m; // hold returned messages for processing
            Response<bool> m2;
            Response<double> volts;

            // Check J2534 API
            //this.Logger.AddDebugMessage(J2534Port.Functions.ToString());

            // Check not already loaded
            if (IsLoaded == true)
            {
                this.Logger.AddDebugMessage("DLL already loaded, unloading before proceeding");
                m2 = CloseLibrary();
                if (m2.Status != ResponseStatus.Success)
                {
                    this.Logger.AddUserMessage("Error closing loaded DLL");
                    return false;
                }
                this.Logger.AddDebugMessage("Existing DLL successfully unloaded.");
            }

            // Connect to requested DLL
            m2 = LoadLibrary(J2534Port.LoadedDevice);
            if (m2.Status != ResponseStatus.Success)
            {
                this.Logger.AddUserMessage("Unable to load the J2534 DLL.");
                return false;
            }
            this.Logger.AddUserMessage("Loaded DLL");

            // Connect to scantool
            m = ConnectTool();
            if (m.Status != ResponseStatus.Success)
            {
                this.Logger.AddUserMessage("Unable to connect to the device.");
                return false;
            }

            this.Logger.AddUserMessage("Connected to the device.");
            
            // Optional.. read API,firmware version ect here
            
            // Read voltage
            volts = ReadVoltage();
            if (volts.Status != ResponseStatus.Success)
            {
                this.Logger.AddDebugMessage("Unable to read battery voltage.");
            }
            else
            {
                this.Logger.AddUserMessage("Battery Voltage is: " + volts.Value.ToString());
            }

            // Set Protocol
            m = ConnectToProtocol(ProtocolID.J1850VPW, BaudRate.J1850VPW_10400, ConnectFlag.NONE);
            if (m.Status != ResponseStatus.Success)
            {
                this.Logger.AddUserMessage("Failed to set protocol, J2534 error code: 0x" + m.Value.ToString("X"));
                return false;
            }
            this.Logger.AddDebugMessage("Protocol Set");

            // Set filter
            m = SetFilter(0xFEFFFF, J2534Device.MessageFilter, 0, TxFlag.NONE, FilterType.PASS_FILTER);
            if (m.Status != ResponseStatus.Success)
            {
                this.Logger.AddUserMessage("Failed to set filter, J2534 error code: 0x" + m.Value.ToString("X2"));
                return false;
            }

            this.Logger.AddDebugMessage("Device initialization complete.");

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
            for (int iterations = 0; iterations < 5; iterations++)
            {
                Message response = await this.ReceiveMessage();
                if (Utility.CompareArraysPart(response.GetBytes(), expected.GetBytes()))
                {
                    return Response.Create(ResponseStatus.Success, response);
                }
                await Task.Delay(100);
            }

            return Response.Create(ResponseStatus.Timeout, (Message)null);
        }

        /// <summary>
        /// Read an network packet from the interface, and return a Response/Message
        /// </summary>
        protected override Task Receive()
        {
            //this.Logger.AddDebugMessage("Trace: Read Network Packet");

            PassThruMsg PassMess = new PassThruMsg();
            int NumMessages = 1;
            IntPtr rxMsgs = Marshal.AllocHGlobal((int)(Marshal.SizeOf(typeof(PassThruMsg)) * NumMessages));

            OBDError = 0; // Clear any previous faults

            Stopwatch sw = new Stopwatch();
            sw.Start();

            do
            {
                NumMessages = 1;
                OBDError = J2534Port.Functions.PassThruReadMsgs((int)ChannelID, rxMsgs, ref NumMessages, ReadTimeout);
                if (OBDError != J2534Err.STATUS_NOERROR)
                {
                    this.Logger.AddDebugMessage("ReadMsgs OBDError: " + OBDError);
                    Marshal.FreeHGlobal(rxMsgs);
                    return Task.FromResult(0);
                }

                PassMess = rxMsgs.AsMsgList(1).Last();
                if ((int)PassMess.RxStatus == (((int)RxStatus.TX_INDICATION_SUCCESS) + ((int)RxStatus.TX_MSG_TYPE)) || (PassMess.RxStatus == RxStatus.START_OF_MESSAGE))
                {
                    continue;
                }
                else
                {
                    byte[] TempBytes = PassMess.GetBytes();
                    // Perform additional filter check if required here... or show to debug
                    break; // Exit loop
                }
            } while (OBDError == J2534Err.STATUS_NOERROR || sw.ElapsedMilliseconds > (long)ReadTimeout);
            sw.Stop();

            Marshal.FreeHGlobal(rxMsgs);

            if (OBDError != J2534Err.STATUS_NOERROR || sw.ElapsedMilliseconds > (long)ReadTimeout)
            {
                this.Logger.AddDebugMessage("ReadMsgs OBDError: " + OBDError);
                return Task.FromResult(0);
            }

            this.Logger.AddDebugMessage("RX: " + PassMess.GetBytes().ToHex());
            this.Enqueue(new Message(PassMess.GetBytes(), PassMess.Timestamp, (ulong)OBDError));
            return Task.FromResult(0);
        }

        /// <summary>
        /// Convert a Message to an J2534 formatted transmit, and send to the interface
        /// </summary>
        private Response<J2534Err> SendNetworkMessage(Message message, TxFlag Flags)
        {
            //this.Logger.AddDebugMessage("Trace: Send Network Packet");

            PassThruMsg TempMsg = new PassThruMsg();
            TempMsg.ProtocolID = Protocol;
            TempMsg.TxFlags = Flags;
            TempMsg.SetBytes(message.GetBytes());

            int NumMsgs = 1;

            IntPtr MsgPtr = TempMsg.ToIntPtr();

            OBDError = J2534Port.Functions.PassThruWriteMsgs((int)ChannelID, MsgPtr, ref NumMsgs, WriteTimeout);
            Marshal.FreeHGlobal(MsgPtr);
            if (OBDError != J2534Err.STATUS_NOERROR)
            {
                // Debug messages here...check why failed..
                return Response.Create(ResponseStatus.Error, OBDError);
            }
            return Response.Create(ResponseStatus.Success, OBDError);
        }
        
        /// <summary>
        /// Send a message, wait for a response, return the response.
        /// </summary>
        public override Task<bool> SendMessage(Message message)
        {
            //this.Logger.AddDebugMessage("Send request called");
            this.Logger.AddDebugMessage("TX: " + message.GetBytes().ToHex());
            Response<J2534Err> MyError = SendNetworkMessage(message,TxFlag.NONE);
            if (MyError.Status != ResponseStatus.Success)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
        
        /// <summary>
        /// Load in dll
        /// </summary>
        private Response<bool> LoadLibrary(J2534.J2534Device TempDevice)
        {
            ToolName = TempDevice.Name;
            J2534Port.LoadedDevice = TempDevice;
            if (J2534Port.Functions.LoadLibrary(J2534Port.LoadedDevice))
            {
                return Response.Create(ResponseStatus.Success, true);
            }
            else
            {
                return Response.Create(ResponseStatus.Error, false);
            }
        }

        /// <summary>
        /// Unload dll
        /// </summary>
        private Response<bool> CloseLibrary()
        {
            if (J2534Port.Functions.FreeLibrary())
            {
                return Response.Create(ResponseStatus.Success, true);
            }
            else
            {
                return Response.Create(ResponseStatus.Error, false);
            }
        }

        /// <summary>
        /// Connects to physical scantool
        /// </summary>
        private Response<J2534Err> ConnectTool()
        {
            DeviceID = 0;
            ChannelID = 0;
            Filters.Clear();
            OBDError = 0;
            OBDError = J2534Port.Functions.PassThruOpen(IntPtr.Zero, ref DeviceID);
            if (OBDError != J2534Err.STATUS_NOERROR)
            {
                IsJ2534Open = false;
                return Response.Create(ResponseStatus.Error, OBDError);
            }
            else
            {
                IsJ2534Open = true;
                return Response.Create(ResponseStatus.Success, OBDError);
            }
        }

        /// <summary>
        /// Disconnects from physical scantool
        /// </summary>
        private Response<J2534Err> DisconnectTool()
        {
            OBDError = J2534Port.Functions.PassThruClose(DeviceID);
            if (OBDError != J2534Err.STATUS_NOERROR)
            {
                // Big problems, do something here
            }
            IsJ2534Open = false;
            return Response.Create(ResponseStatus.Success, OBDError);
        }

        /// <summary>
        /// Keep record if DLL has been loaded
        /// </summary>
        public bool IsLoaded
        {
            get { return J2534Port.Functions.IsLoaded; }
            set {; }
        }

        /// <summary>
        /// Connect to selected protocol
        /// Must provide protocol, speed, connection flags, recommended optional is pins
        /// </summary>
        private Response<J2534Err> ConnectToProtocol(ProtocolID ReqProtocol, BaudRate Speed, ConnectFlag ConnectFlags)
        {
            OBDError = J2534Port.Functions.PassThruConnect(DeviceID, ReqProtocol,  ConnectFlags,  Speed, ref ChannelID);
            if (OBDError != J2534Err.STATUS_NOERROR)
            {
                return Response.Create(ResponseStatus.Error, OBDError);
            }
            Protocol = ReqProtocol;
            IsProtocolOpen = true;
            return Response.Create(ResponseStatus.Success, OBDError);
        }

        /// <summary>
        /// Disconnect from protocol
        /// </summary>
        private Response<J2534Err> DisconnectFromProtocol()
        {
            OBDError = J2534Port.Functions.PassThruDisconnect((int)ChannelID);
            if (OBDError != J2534Err.STATUS_NOERROR)
            {
                return Response.Create(ResponseStatus.Error, OBDError);
            }
            IsProtocolOpen = false;
            return Response.Create(ResponseStatus.Success, OBDError);
        }

        /// <summary>
        /// Read battery voltage
        /// </summary>
        public Response<double> ReadVoltage()
        {
            double Volts = 0;
            int VoltsAsInt = 0;
            OBDError = J2534Port.Functions.ReadBatteryVoltage((int)DeviceID, ref VoltsAsInt);
            if (OBDError != J2534Err.STATUS_NOERROR)
            {
                return Response.Create(ResponseStatus.Error, Volts);
            }
            else
            {
                Volts = VoltsAsInt / 1000.0;
                return Response.Create(ResponseStatus.Success, Volts);
            }
        }

        /// <summary>
        /// Set filter
        /// </summary>
        private Response<J2534Err> SetFilter(UInt32 Mask,UInt32 Pattern,UInt32 FlowControl,TxFlag txflag,FilterType Filtertype)
        {
            PassThruMsg maskMsg;
            PassThruMsg patternMsg;

            IntPtr MaskPtr;
            IntPtr PatternPtr;
            IntPtr FlowPtr;

            maskMsg = new PassThruMsg(Protocol, txflag, new Byte[] { (byte)(0xFF & (Mask >> 16)), (byte)(0xFF & (Mask >> 8)), (byte)(0xFF & Mask) });
            patternMsg = new PassThruMsg(Protocol, txflag, new Byte[] { (byte)(0xFF & (Pattern >> 16)), (byte)(0xFF & (Pattern >> 8)), (byte)(0xFF & Pattern) });
            MaskPtr = maskMsg.ToIntPtr();
            PatternPtr = patternMsg.ToIntPtr();
            FlowPtr = IntPtr.Zero;

            int tempfilter = 0;
            OBDError = J2534Port.Functions.PassThruStartMsgFilter((int)ChannelID, Filtertype, MaskPtr, PatternPtr, FlowPtr, ref tempfilter);

            Marshal.FreeHGlobal(MaskPtr);
            Marshal.FreeHGlobal(PatternPtr);
            Marshal.FreeHGlobal(FlowPtr);
            if (OBDError != J2534Err.STATUS_NOERROR)
            {
                return Response.Create(ResponseStatus.Error, OBDError);
            }
            Filters.Add((ulong)tempfilter);
            return Response.Create(ResponseStatus.Success, OBDError);
        }

        /// <summary>
        /// Set the interface to low (false) or high (true) speed
        /// </summary>
        /// <remarks>
        /// The caller must also tell the PCM to switch speeds
        /// </remarks>
        protected override Task<bool> SetVpwSpeedInternal(VpwSpeed newSpeed)
        {
            if (newSpeed == VpwSpeed.Standard)
            {
                this.Logger.AddDebugMessage("J2534 setting VPW 1X");
                // Disconnect from current protocol
                DisconnectFromProtocol();

                // Connect at new speed
                ConnectToProtocol(ProtocolID.J1850VPW, BaudRate.J1850VPW_10400, ConnectFlag.NONE);

                // Set Filter
                SetFilter(0xFEFFFF, J2534Device.MessageFilter, 0, TxFlag.NONE, FilterType.PASS_FILTER);
                //if (m.Status != ResponseStatus.Success)
                //{
                //    this.Logger.AddDebugMessage("Failed to set filter, J2534 error code: 0x" + m.Value.ToString("X2"));
                //    return false;
                //}


            }
            else
            {
                this.Logger.AddDebugMessage("J2534 setting VPW 4X");
                // Disconnect from current protocol
                DisconnectFromProtocol();

                // Connect at new speed
                ConnectToProtocol(ProtocolID.J1850VPW, BaudRate.J1850VPW_41600, ConnectFlag.NONE);

                // Set Filter
                SetFilter(0xFEFFFF, J2534Device.MessageFilter, 0, TxFlag.NONE, FilterType.PASS_FILTER);

            }

            return Task.FromResult(true);
        }

        public override void ClearMessageBuffer()
        {
            J2534Port.Functions.ClearRxBuffer((int)DeviceID);
            J2534Port.Functions.ClearTxBuffer((int)DeviceID);
        }
    }
}
