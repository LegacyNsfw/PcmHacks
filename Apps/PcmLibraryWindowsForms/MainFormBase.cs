using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PcmHacking
{
    public partial class MainFormBase : Form, ILogger
    {
        /// <summary>
        /// The Vehicle object is our interface to the car. It has the device, the message generator, and the message parser.
        /// </summary>
        private Vehicle vehicle;
        protected Vehicle Vehicle { get { return this.vehicle; } }
               
        public virtual void AddDebugMessage(string message) { }
        public virtual void AddUserMessage(string message) { }
        public virtual void ResetLogs() { }

        public virtual string GetAppNameAndVersion() { return "MainFormBase.GetAppNameAndVersion is not implemented"; }

        protected virtual void EnableInterfaceSelection() { }
        protected virtual void EnableUserInput() { }
        protected virtual void DisableUserInput() { }

        protected virtual void NoDeviceSelected()
        {
            // disable re-init button
            // set device name to "no device selected"
        }

        protected virtual void ValidDeviceSelected(string deviceName)
        {
            // enable re-init button
            // show device name
        }
        
        /// <summary>
        /// Handle clicking the "Select Interface" button
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HandleSelectButtonClick()
        {
            if (this.vehicle != null)
            {
                this.vehicle.Dispose();
                this.vehicle = null;
            }

            DevicePicker picker = new DevicePicker(this);
            DialogResult result = picker.ShowDialog();
            if (result == DialogResult.OK)
            {
                Configuration.DeviceCategory = picker.DeviceCategory;
                Configuration.J2534DeviceType = picker.J2534DeviceType;
                Configuration.SerialPort = picker.SerialPort;
                Configuration.SerialPortDeviceType = picker.SerialPortDeviceType;
            }

            return await this.ResetDevice();
        }

        /// <summary>
        /// Close the old interface device and open a new one.
        /// </summary>
        protected async Task<bool> ResetDevice()
        {
            if (this.vehicle != null)
            {
                this.vehicle.Dispose();
                this.vehicle = null;
            }

            Device device = DeviceFactory.CreateDeviceFromConfigurationSettings(this);
            if (device == null)
            {
                this.NoDeviceSelected();
                this.DisableUserInput();
                this.EnableInterfaceSelection();
                return false;
            }

            Protocol protocol = new Protocol();
            this.vehicle = new Vehicle(
                device,
                protocol,
                this,
                new ToolPresentNotifier(device, protocol, this));
            if (!await this.InitializeCurrentDevice())
            {
                this.vehicle = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Initialize the current device.
        /// </summary>
        protected async Task<bool> InitializeCurrentDevice()
        {
            this.DisableUserInput();

            if (this.vehicle == null)
            {
                this.EnableInterfaceSelection();
                return false;
            }

            this.ResetLogs();

            this.AddUserMessage(GetAppNameAndVersion());

            try
            {
                // TODO: this should not return a boolean, it should just throw 
                // an exception if it is not able to initialize the device.
                bool initialized = await this.vehicle.ResetConnection();
                if (!initialized)
                {
                    this.AddUserMessage("Unable to initialize " + this.vehicle.DeviceDescription);
                    this.EnableInterfaceSelection();
                    return false;
                }
            }
            catch (Exception exception)
            {
                this.AddUserMessage("Unable to initialize " + this.vehicle.DeviceDescription);
                this.AddDebugMessage(exception.ToString());
                this.EnableInterfaceSelection();
                return false;
            }

            this.ValidDeviceSelected(this.vehicle.DeviceDescription);
            this.EnableUserInput();
            return true;
        }
    }
}
