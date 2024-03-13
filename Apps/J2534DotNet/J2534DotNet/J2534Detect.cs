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
using System.Collections.Generic;
using Microsoft.Win32;

namespace J2534DotNet
{
    static public class J2534Detect
    {
        private const string PASSTHRU_REGISTRY_PATH = "Software\\PassThruSupport.04.04";
        private const string PASSTHRU_REGISTRY_PATH_6432 = "Software\\Wow6432Node\\PassThruSupport.04.04";

        static public List<J2534Device> ListDevices()
        {
            List<J2534Device> j2534Devices = new List<J2534Device>();
            RegistryKey myKey = Registry.LocalMachine.OpenSubKey(PASSTHRU_REGISTRY_PATH, false);
            if (myKey == null)
            {
                myKey = Registry.LocalMachine.OpenSubKey(PASSTHRU_REGISTRY_PATH_6432, false);
                if (myKey == null)
                    return j2534Devices;
            }
            string[] devices = myKey.GetSubKeyNames();
            foreach (string device in devices)
            {
                J2534Device tempDevice = new J2534Device();
                RegistryKey deviceKey = myKey.OpenSubKey(device);
                if(deviceKey == null)
                    continue;
                tempDevice.Vendor = (string)deviceKey.GetValue("Vendor","");
                tempDevice.Name = (string)deviceKey.GetValue("Name","");
                tempDevice.ConfigApplication = (string)deviceKey.GetValue("ConfigApplication", "");
                tempDevice.FunctionLibrary = (string)deviceKey.GetValue("FunctionLibrary", "");
                
                tempDevice.CANChannels = (int)deviceKey.GetValue("CAN",0);
                tempDevice.ISO15765Channels = (int)deviceKey.GetValue("ISO15765",0);
                tempDevice.J1850PWMChannels = (int)deviceKey.GetValue("J1850PWM",0);
                tempDevice.J1850VPWChannels = (int)deviceKey.GetValue("J1850VPW", 0);
                tempDevice.ISO9141Channels = (int)deviceKey.GetValue("ISO9141", 0);
                tempDevice.ISO14230Channels = (int)deviceKey.GetValue("ISO14230", 0);
                tempDevice.SCI_A_ENGINEChannels = (int)deviceKey.GetValue("SCI_A_ENGINE", 0);
                tempDevice.SCI_A_TRANSChannels = (int)deviceKey.GetValue("SCI_A_TRANS", 0);
                tempDevice.SCI_B_ENGINEChannels = (int)deviceKey.GetValue("SCI_B_ENGINE", 0);
                tempDevice.SCI_B_TRANSChannels = (int)deviceKey.GetValue("SCI_B_TRANS", 0);

                j2534Devices.Add(tempDevice);
            }

            return j2534Devices;
        }
    }
}
