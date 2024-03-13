#region Copyright (c) 2010, Michael Kelly
/* 
 * Copyright (c) 2010, Michael Kelly
 * michael.e.kelly@gmail.com
 * http://michael-kelly.com/
 * 
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 * Neither the name of the organization nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 */
#endregion License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using J2534DotNet;

namespace Sample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /*
         *  Example 1:
         *      Detect J2534 devices
         * 
         */
        private void CmdDetectDevicesClick(object sender, EventArgs e)
        {
            // Calling J2534.GetAvailableDevices() will return a list of installed J2534 devices
            List<J2534Device> availableJ2534Devices = J2534Detect.ListDevices();
            if (availableJ2534Devices.Count == 0)
            {
                MessageBox.Show("Could not find any installed J2534 devices.");
                return;
            }

            foreach (J2534Device device in availableJ2534Devices)
            {
                txtDevices.Text += device.Name + ", " + device.Vendor + "\r\n\r\n";
                txtDevices.Text += "\tConfig Application:\t" + device.ConfigApplication + "\r\n";
                txtDevices.Text += "\tFunction Library:\t" + device.FunctionLibrary + "\r\n\r\n";
                txtDevices.Text += "\tProtocol\t\tChannels\r\n";
                txtDevices.Text += "\tCAN\t\t" + device.CANChannels + "\r\n";
                txtDevices.Text += "\tISO15765\t" + device.ISO15765Channels + "\r\n";
                txtDevices.Text += "\tISO14230\t" + device.ISO14230Channels + "\r\n";
                txtDevices.Text += "\tISO9141\t\t" + device.ISO9141Channels + "\r\n";
                txtDevices.Text += "\tJ1850PWM\t" + device.J1850PWMChannels + "\r\n";
                txtDevices.Text += "\tJ1850PWM\t" + device.J1850VPWChannels + "\r\n";
                txtDevices.Text += "\tSCI_A_ENGINE\t" + device.SCI_A_ENGINEChannels + "\r\n";
                txtDevices.Text += "\tSCI_A_TRANS\t" + device.SCI_A_TRANSChannels + "\r\n";
                txtDevices.Text += "\tSCI_B_ENGINE\t" + device.SCI_B_ENGINEChannels + "\r\n";
                txtDevices.Text += "\tSCI_B_TRANS\t" + device.SCI_B_TRANSChannels + "\r\n\r\n";
            }
        }

        /*
         * 
         *  Example 2:
         *      Use the J2534 protocol to send and receive a message (w/o error checking)
         * 
         */
        private void SendReceiveNoErrorChecking(object sender, EventArgs e)
        {
            J2534 passThru = new J2534();

            // Find all of the installed J2534 passthru devices
            List<J2534Device> availableJ2534Devices = J2534Detect.ListDevices();

            // We will always choose the first J2534 device in the list, if there are multiple devices
            //   installed, you should do something more intelligent.
            passThru.LoadLibrary(availableJ2534Devices[0]);

            // Attempt to open a communication link with the pass thru device
            int deviceId = 0;
            passThru.Open(ref deviceId);

            // Open a new channel configured for ISO15765 (CAN)
            int channelId = 0;
            passThru.Connect(deviceId, ProtocolID.ISO15765, ConnectFlag.NONE, BaudRate.ISO15765, ref channelId);

            // Set up a message filter to watch for response messages
            int filterId = 0;
            PassThruMsg maskMsg = new PassThruMsg(
                ProtocolID.ISO15765,
                TxFlag.ISO15765_FRAME_PAD,
                new byte[] { 0xff, 0xff, 0xff, 0xff });
            PassThruMsg patternMsg = new PassThruMsg(
                ProtocolID.ISO15765,
                TxFlag.ISO15765_FRAME_PAD,
                new byte[] { 0x00, 0x00, 0x07, 0xE8});
            PassThruMsg flowControlMsg = new PassThruMsg(
                ProtocolID.ISO15765,
                TxFlag.ISO15765_FRAME_PAD,
                new byte[] { 0x00, 0x00, 0x07, 0xE0});
            passThru.StartMsgFilter(channelId, FilterType.FLOW_CONTROL_FILTER, ref maskMsg, ref patternMsg, ref flowControlMsg, ref filterId);

            // Clear out the response buffer so we know we're getting the freshest possible data
            passThru.ClearRxBuffer(channelId);

            // Finally we can send the message!
            PassThruMsg txMsg = new PassThruMsg(
                ProtocolID.ISO15765,
                TxFlag.ISO15765_FRAME_PAD,
                new byte[] { 0x00, 0x00, 0x07, 0xdf, 0x01, 0x00 });
            int numMsgs = 1;
            passThru.WriteMsgs(channelId, ref txMsg, ref numMsgs, 50);

            // Read messages in a loop until we either timeout or we receive data
            List<PassThruMsg> rxMsgs = new List<PassThruMsg>();
            J2534Err status = J2534Err.STATUS_NOERROR;
            numMsgs = 1;
            while (J2534Err.STATUS_NOERROR == status)
                status = passThru.ReadMsgs(channelId, ref rxMsgs, ref numMsgs, 200);

            // If we received data, we want to extract the data of interest.  I'm removing the reflection of the transmitted message.
            List<byte> responseData;
            if ((J2534Err.ERR_BUFFER_EMPTY == status || J2534Err.ERR_TIMEOUT == status) && rxMsgs.Count > 1)
            {
                responseData = rxMsgs[rxMsgs.Count - 1].Data.ToList();
                responseData.RemoveRange(0, txMsg.Data.Length);
            }

            //
            //
            // Now do something with the data!
            //
            //

            // Disconnect this channel
            passThru.Disconnect(channelId);

            // When we are done with the device, we can free the library.
            passThru.FreeLibrary();
        }

        /*
         * 
         *  Use the J2534 protocol to read voltage
         * 
         */
        private void CmdReadVoltageClick(object sender, EventArgs e)
        {
            J2534 passThru = new J2534();
            double voltage = 0;

            // Find all of the installed J2534 passthru devices
            List<J2534Device> availableJ2534Devices = J2534Detect.ListDevices();
            if (availableJ2534Devices.Count == 0)
            {
                MessageBox.Show("Could not find any installed J2534 devices.");
                return;
            }

            // We will always choose the first J2534 device in the list, if there are multiple devices
            //   installed, you should do something more intelligent.
            passThru.LoadLibrary(availableJ2534Devices[0]);

            ObdComm comm = new ObdComm(passThru);
            if (!comm.DetectProtocol())
            {
                MessageBox.Show(String.Format("Error connecting to device. Error: {0}", comm.GetLastError()));
                comm.Disconnect();
                return;
            }
            if (!comm.GetBatteryVoltage(ref voltage))
            {
                MessageBox.Show(String.Format("Error reading voltage.  Error: {0}", comm.GetLastError()));
                comm.Disconnect();
                return;
            }
            comm.Disconnect();

            // When we are done with the device, we can free the library.
            passThru.FreeLibrary();
            txtVoltage.Text = voltage + @" V";
        }

        private void CmdReadVinClick(object sender, EventArgs e)
        {
            J2534 passThru = new J2534();
            string vin = "";

            // Find all of the installed J2534 passthru devices
            List<J2534Device> availableJ2534Devices = J2534Detect.ListDevices();
            if (availableJ2534Devices.Count == 0)
            {
                MessageBox.Show("Could not find any installed J2534 devices.");
                return;
            }

            // We will always choose the first J2534 device in the list, if there are multiple devices
            //   installed, you should do something more intelligent.
            passThru.LoadLibrary(availableJ2534Devices[0]);

            ObdComm comm = new ObdComm(passThru);
            if (!comm.DetectProtocol())
            {
                MessageBox.Show(String.Format("Error connecting to device. Error: {0}", comm.GetLastError()));
                comm.Disconnect();
                return;
            }
            if (!comm.GetVin(ref vin))
            {
                MessageBox.Show(String.Format("Error reading VIN.  Error: {0}", comm.GetLastError()));
                comm.Disconnect();
                return;
            }
            comm.Disconnect();

            // When we are done with the device, we can free the library.
            passThru.FreeLibrary();
            txtReadVin.Text = vin;
        }
    }
}
