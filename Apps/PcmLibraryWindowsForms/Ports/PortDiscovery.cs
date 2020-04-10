using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using Microsoft.Win32;

namespace PcmHacking
{
    /// <summary>
    /// Largely based on Muno's answer here: https://stackoverflow.com/questions/2837985/getting-serial-port-information
    /// </summary>
    class PortDiscovery
    {
        public static IEnumerable<SerialPortInfo> GetPorts(ILogger logger)
        {
            List<SerialPortInfo> result = new List<SerialPortInfo>();
            ManagementClass processClass = new ManagementClass("Win32_PnPEntity");
            ManagementObjectCollection Ports = processClass.GetInstances();
            foreach (ManagementObject portManagementObject in Ports)
            {
                var classGuid = portManagementObject.GetPropertyValue("ClassGuid") as string ?? string.Empty;
                if (classGuid == "{4d36e978-e325-11ce-bfc1-08002be10318}")
                {
                    var portInfo = new SerialPortInfo(portManagementObject, logger);
                    if (portInfo.PortName == null)
                    {
                        continue;
                    }

                    if (portInfo.PortNumber == 0)
                    {
                        continue;
                    }

                    result.Add(portInfo);
                }
            }

            return result.OrderBy(x => x.PortNumber);
        }
    }

    public class SerialPortInfo
    {
        /* 
        int Availability;
        string Caption;
        string ClassGuid;
        string[] CompatibleID;
        int ConfigManagerErrorCode;
        bool ConfigManagerUserConfig;
        string CreationClassName;
        string Description;
        bool ErrorCleared;
        string ErrorDescription;
        string[] HardwareID;
        DateTime InstallDate;
        int LastErrorCode;
        string Manufacturer;
        string PNPClass;
        string PNPDeviceID;
        int[] PowerManagementCapabilities;
        bool PowerManagementSupported;
        bool Present;
        string Service;
        string Status;
        int StatusInfo;
        string SystemCreationClassName;
        string SystemName;
        */

        private string Name;

        public string PortName { get; private set; }
        public string DeviceID { get; private set; }
        public int PortNumber { get; private set; }

        public SerialPortInfo(ManagementObject property, ILogger logger)
        {
            /* These are not needed but are retained here in case they might be useful for diagnosing surprises.
            this.Availability = property.GetPropertyValue("Availability") as int? ?? 0;
            this.Caption = property.GetPropertyValue("Caption") as string ?? string.Empty;
            this.ClassGuid = property.GetPropertyValue("ClassGuid") as string ?? string.Empty;
            this.CompatibleID = property.GetPropertyValue("CompatibleID") as string[] ?? new string[] { };
            this.ConfigManagerErrorCode = property.GetPropertyValue("ConfigManagerErrorCode") as int? ?? 0;
            this.ConfigManagerUserConfig = property.GetPropertyValue("ConfigManagerUserConfig") as bool? ?? false;
            this.CreationClassName = property.GetPropertyValue("CreationClassName") as string ?? string.Empty;
            this.Description = property.GetPropertyValue("Description") as string ?? string.Empty;
            this.ErrorCleared = property.GetPropertyValue("ErrorCleared") as bool? ?? false;
            this.ErrorDescription = property.GetPropertyValue("ErrorDescription") as string ?? string.Empty;
            this.HardwareID = property.GetPropertyValue("HardwareID") as string[] ?? new string[] { };
            this.InstallDate = property.GetPropertyValue("InstallDate") as DateTime? ?? DateTime.MinValue;
            this.LastErrorCode = property.GetPropertyValue("LastErrorCode") as int? ?? 0;
            this.Manufacturer = property.GetPropertyValue("Manufacturer") as string ?? string.Empty;
            this.PNPClass = property.GetPropertyValue("PNPClass") as string ?? string.Empty;
            this.PNPDeviceID = property.GetPropertyValue("PNPDeviceID") as string ?? string.Empty;
            this.PowerManagementCapabilities = property.GetPropertyValue("PowerManagementCapabilities") as int[] ?? new int[] { };
            this.PowerManagementSupported = property.GetPropertyValue("PowerManagementSupported") as bool? ?? false;
            this.Present = property.GetPropertyValue("Present") as bool? ?? false;
            this.Service = property.GetPropertyValue("Service") as string ?? string.Empty;
            this.Status = property.GetPropertyValue("Status") as string ?? string.Empty;
            this.StatusInfo = property.GetPropertyValue("StatusInfo") as int? ?? 0;
            this.SystemCreationClassName = property.GetPropertyValue("SystemCreationClassName") as string ?? string.Empty;
            this.SystemName = property.GetPropertyValue("SystemName") as string ?? string.Empty;
            */

            this.Name = property.GetPropertyValue("Name") as string ?? string.Empty;
            this.DeviceID = property.GetPropertyValue("DeviceID") as string ?? string.Empty;
            this.PortName = GetPortName(this.DeviceID, logger);

            if (!string.IsNullOrEmpty(this.PortName) &&
                !this.Name.Contains("(LPT") && 
                this.PortName.StartsWith("COM") && 
                this.PortName.Length > 3)
            {
                int number;
                int.TryParse(this.PortName.Substring(3), out number);
                this.PortNumber = number;
            }
            else
            {
                if (!this.PortName.StartsWith("LPT"))
                {
                    logger.AddDebugMessage(
                        string.Format(
                            "Unable to get port number for '{0}' / '{1}' with port name '{2}'",
                            this.Name,
                            this.DeviceID,
                            this.PortName));
                }
            }
        }

        private static string GetPortName(string deviceId, ILogger logger)
        {
            try
            {
                string key = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\" + deviceId + @"\Device Parameters";
                return Registry.GetValue(key, "PortName", null) as string;
            }
            catch (Exception exception)
            {
                logger.AddDebugMessage(
                    string.Format(
                        "GetPortName failed for '{0}': {1}: {2}",
                        deviceId,
                        exception.GetType().Name,
                        exception.Message));
                return null;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
