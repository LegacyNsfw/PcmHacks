using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// From the application's perspective, this class is the API to the vehicle.
    /// </summary>
    public partial class Vehicle : IDisposable
    {
        /// <summary>
        /// Start logging
        /// </summary>
        public async Task<bool> StartLogging(LogProfile profile)
        {
            byte dpid = 0xFE;
            List<byte> dpids = new List<byte>();
            foreach(ParameterGroup group in profile.ParameterGroups)
            {
                int position = 1;
                foreach(ProfileParameter parameter in group.Parameters)
                {
                    Message configurationMessage = this.protocol.ConfigureDynamicData(
                        dpid,
                        parameter.DefineBy,
                        position,
                        parameter.ByteCount,
                        parameter.Address);

                    if (!await this.SendMessage(configurationMessage))
                    {
                        return false;
                    }

                    // TODO: sanity check the response.

                    position += parameter.ByteCount;
                }
                dpids.Add(dpid);
                dpid--;
            }

            Message startMessage = this.protocol.BeginLogging(dpids.ToArray());
            if (!await this.SendMessage(startMessage))
            {
                return false;
            }

            return true;
        }

        public async Task<bool> StartLogging_Old()
        {
            // NOT SUPPORTED (in my ROM anyway, need to try others)
            // 19F3 - transmission temprature
            // 1602 - oil temperature
            // 125D - knock retard
            // 19F5 - current gear
            // 125E - knock count, two-byte

            // Configure logging parameters
            // DPID numbers 0xF2-0xFE all work
            // 0xFE is highest priority
            // 0xFA is very slow
            byte dpid1 = 0xFE;


            // Load 2 bytes of SAE RPM to DPID positions 1 and 2
            Message message = this.protocol.ConfigureDynamicData(dpid1, DefineBy.Pid, 1, 2, 0x000C);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 2 bytes from SAE MAF to positions 3 and 4
            message = this.protocol.ConfigureDynamicData(dpid1, DefineBy.Pid, 3, 2, 0x0010);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of SAE MAP to position 5
            message = this.protocol.ConfigureDynamicData(dpid1, DefineBy.Pid, 5, 1, 0x000B);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of SAE TPS to position 6
            message = this.protocol.ConfigureDynamicData(dpid1, DefineBy.Pid, 6, 1, 0x0011);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            byte dpid2 = 0xFD;

            // Load SAE IAT to DPID position 1
            message = this.protocol.ConfigureDynamicData(dpid2, DefineBy.Pid, 1, 1, 0x000F);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // Load SAE coolant temperature to position 2
            message = this.protocol.ConfigureDynamicData(dpid2, DefineBy.Pid, 2, 1, 0x0005);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of SAE trans temp to position 3 (this is raw sensor value.)
            message = this.protocol.ConfigureDynamicData(dpid2, DefineBy.Pid, 3, 1, 0x19AD);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of SAE speed (KMH) to position 4 
            message = this.protocol.ConfigureDynamicData(dpid2, DefineBy.Pid, 4, 1, 0x000D);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of GM Knock to position 5
            message = this.protocol.ConfigureDynamicData(dpid2, DefineBy.Pid, 5, 1, 0x11A6);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of GM fuel status to position 6 
            message = this.protocol.ConfigureDynamicData(dpid2, DefineBy.Pid, 6, 1, 0x1105);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            /*
            byte dpid3 = 0xFC;

            // Load left LTFT to position 1
            message = this.protocol.ConfigureDynamicData(dpid3, DefineBy.Pid, 1, 1, 0x0007);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // Load right LTFT to position 2
            message = this.protocol.ConfigureDynamicData(dpid3, DefineBy.Pid, 2, 1, 0x0009);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of Gm Target AFR to position 3 (0:1-25.5:1)
            message = this.protocol.ConfigureDynamicData(dpid3, DefineBy.Pid, 3, 1, 0x119E);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of GM battery voltage to position 4
            message = this.protocol.ConfigureDynamicData(dpid3, DefineBy.Pid, 4, 1, 0x1141);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of GM current BLM cell to position 5 
            message = this.protocol.ConfigureDynamicData(dpid3, DefineBy.Pid, 5, 1, 0x1190);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of "normalized TPS" (so what's the other TPS parmeter?) to position 6
            message = this.protocol.ConfigureDynamicData(dpid3, DefineBy.Pid, 6, 1, 0x1151);
            if (!await this.SendMessage(message))
            {
                return false;
            }
            */

            // Start logging
            message = this.protocol.BeginLogging(dpid1, dpid2);//, dpid3);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Query the PCM's Serial Number.
        /// </summary>
        public async Task<string[]> ReadLogRow()
        {
            Message message = await this.ReceiveMessage();
            return new string[0];
        }
    }
}
