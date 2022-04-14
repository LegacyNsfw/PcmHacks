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
        /// Impolite, but it needs to be short enough to fit in the device-description box.
        /// </summary>
        private const string selectAnotherDevice = "Select another device.";

        /// <summary>
        /// The Vehicle object is our interface to the car. It has the device, the message generator, and the message parser.
        /// </summary>
        private Vehicle vehicle;
        protected Vehicle Vehicle { get { return this.vehicle; } }

        public virtual void AddDebugMessage(string message) { }
        public virtual void AddUserMessage(string message) { }
        public virtual void StatusUpdateActivity(string activity) { }
        public virtual void StatusUpdateTimeRemaining(string remaining) { }
        public virtual void StatusUpdatePercentDone(string percent) { }
        public virtual void StatusUpdateRetryCount(string retries) { }
        public virtual void StatusUpdateProgressBar(double completed, bool visible) { }
        public virtual void StatusUpdateKbps(string Kbps) { }
        public virtual void StatusUpdateReset() { }
        public virtual void ResetLogs() { }

        public virtual string GetAppNameAndVersion() { return "MainFormBase.GetAppNameAndVersion is not implemented"; }

        protected virtual void EnableInterfaceSelection() { }
        protected virtual void EnableUserInput() { }
        protected virtual void DisableUserInput() { }

        protected virtual void SetSelectedDeviceText(string message)
        {

        }

        protected virtual void NoDeviceSelected()
        {
            // disable re-init button
            // set device name to "no device selected"
        }

        protected virtual async Task ValidDeviceSelectedAsync(string deviceName)
        {
            // enable re-init button
            // show device name

            // This is just here to suppress a compiler warning.
            await Task.CompletedTask;
        }

        /// <summary>
        /// Handle clicking the "Select Interface" button
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HandleSelectButtonClick()
        {
            using (DevicePicker picker = new DevicePicker(this))
            {
                DialogResult result = picker.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (picker.DeviceCategory == DeviceConfiguration.Constants.DeviceCategorySerial)
                    {
                        if (string.IsNullOrEmpty(picker.SerialPort))
                        {
                            return false;
                        }

                        if (string.IsNullOrEmpty(picker.SerialPortDeviceType))
                        {
                            return false;
                        }
                    }

                    if (picker.DeviceCategory == DeviceConfiguration.Constants.DeviceCategoryJ2534)
                    {
                        if (string.IsNullOrEmpty(picker.J2534DeviceType))
                        {
                            return false;
                        }
                    }

                    DeviceConfiguration.Settings.Enable4xReadWrite = picker.Enable4xReadWrite;
                    DeviceConfiguration.Settings.DeviceCategory = picker.DeviceCategory;
                    DeviceConfiguration.Settings.J2534DeviceType = picker.J2534DeviceType;
                    DeviceConfiguration.Settings.SerialPort = picker.SerialPort;
                    DeviceConfiguration.Settings.SerialPortDeviceType = picker.SerialPortDeviceType;
                    DeviceConfiguration.Settings.Save();
                    return await this.ResetDevice();
                }
            }
            return false;
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
                this.Invoke((MethodInvoker)delegate()
                {
                    this.NoDeviceSelected();
                    this.SetSelectedDeviceText(selectAnotherDevice);
                    this.DisableUserInput();
                    this.EnableInterfaceSelection();
                });
                return false;
            }

            this.Invoke((MethodInvoker)delegate ()
            {
                this.SetSelectedDeviceText("Connecting, please wait...");
            });

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
            if (this.vehicle == null)
            {
                return false;
            }

            this.Invoke((MethodInvoker)delegate ()
            {
                this.DisableUserInput();
                this.ResetLogs();
            });

            this.AddUserMessage(GetAppNameAndVersion());
            this.AddUserMessage(DateTime.Now.ToString("dddd, MMMM dd yyyy @hh:mm:ss:ff"));

            try
            {
                // TODO: this should not return a boolean, it should just throw 
                // an exception if it is not able to initialize the device.
                Task<bool> initializationTask = this.vehicle.ResetConnection();
                bool completed = await initializationTask.AwaitWithTimeout(TimeSpan.FromSeconds(5));
                if (!completed)
                {
                    throw new TimeoutException("Vehicle.ResetConnection timed out.");
                }

                if (!initializationTask.Result)
                {
                    this.AddUserMessage("Unable to initialize " + this.vehicle.DeviceDescription);

                    this.Invoke((MethodInvoker)delegate ()
                    {
                        this.NoDeviceSelected();
                        this.SetSelectedDeviceText(selectAnotherDevice);
                        this.EnableInterfaceSelection();
                    });
                    return false;
                }
            }
            catch (Exception exception)
            {
                this.AddUserMessage("Unable to initialize " + this.vehicle.DeviceDescription);

                this.AddDebugMessage(exception.ToString());
                this.Invoke((MethodInvoker)delegate ()
                {
                    this.NoDeviceSelected();
                    this.SetSelectedDeviceText(selectAnotherDevice);
                    this.EnableInterfaceSelection();
                });
                return false;
            }

            this.Invoke((MethodInvoker)delegate ()
            {
                if (!this.vehicle.Supports4X)
                {
                    DeviceConfiguration.Settings.Enable4xReadWrite = true;
                    DeviceConfiguration.Settings.Save();
                }
                this.vehicle.Enable4xReadWrite = DeviceConfiguration.Settings.Enable4xReadWrite;
            });

            await this.ValidDeviceSelectedAsync(this.vehicle.DeviceDescription);

            this.Invoke((MethodInvoker)delegate ()
            {
                this.EnableUserInput();
            });
            return true;
        }
    }
}
