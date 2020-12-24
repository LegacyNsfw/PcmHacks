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
        /// Prepare the PCM to begin sending collections of parameters.
        /// </summary>
        public async Task<DpidCollection> ConfigureDpids(DpidConfiguration dpidConfiguration, uint osid)
        {
            List<byte> dpids = new List<byte>();

            foreach (ParameterGroup group in dpidConfiguration.ParameterGroups)
            {
                int position = 1;
                foreach (LogColumn column in group.LogColumns)
                {
                    PidParameter pidParameter = column.Parameter as PidParameter;
                    RamParameter ramParameter = column.Parameter as RamParameter;
                    int byteCount;

                    if (pidParameter != null)
                    {
                        Message configurationMessage = this.protocol.ConfigureDynamicData(
                            (byte)group.Dpid,
                            DefineBy.Pid,
                            position,
                            pidParameter.ByteCount,
                            pidParameter.PID);

                        if (!await this.SendMessage(configurationMessage))
                        {
                            return null;
                        }

                        byteCount = pidParameter.ByteCount;
                    }
                    else if (ramParameter != null)
                    {
                        uint address;
                        if (ramParameter.TryGetAddress(osid, out address))
                        {
                            Message configurationMessage = this.protocol.ConfigureDynamicData(
                                (byte)group.Dpid,
                                DefineBy.Address,
                                position,
                                ramParameter.ByteCount,
                                address);

                            if (!await this.SendMessage(configurationMessage))
                            {
                                return null;
                            }

                            byteCount = ramParameter.ByteCount;
                        }
                        else
                        {
                            this.logger.AddUserMessage(
                                string.Format("Parameter {0} is not defined for PCM {1}",
                                ramParameter.Name,
                                osid));
                            byteCount = 0;
                        }
                    }
                    else
                    {
                        throw new Exception("Why does this ParameterGroup contain a " + column.Parameter.GetType().Name + "?");
                    }

                    // Wait for a success or fail message.
                    // TODO: move this into the protocol layer.
                    for (int attempt = 0; attempt < 3; attempt++)
                    {
                        Message responseMessage = await this.ReceiveMessage();

                        if (responseMessage == null)
                        {
                            continue;
                        }

                        if (responseMessage.Length < 5)
                        {
                            continue;
                        }

                        if (responseMessage[3] == 0x6C)
                        {
                            this.logger.AddDebugMessage("Configured " + column.ToString());
                            break;
                        }

                        if (responseMessage[3] == 0x7F && responseMessage[4] == 0x2C)
                        {
                            this.logger.AddUserMessage("Unable to configure " + column.ToString());
                            break;
                        }
                    }


                    position += byteCount;
                }
                dpids.Add((byte)group.Dpid);
            }

            return new DpidCollection(dpids.ToArray());
        }

        /// <summary>
        /// I was hoping to invoke this only once, after ConfigureDpids (which was 
        /// supposed to be named "BeginLogging") but I can't get the devices to 
        /// relay information without periodically requesting it.
        /// 
        /// See also the FAST_LOGGING code in the Logger class and Protocol.Logging.cs.
        /// </summary>
        public async Task<bool> RequestDpids(DpidCollection dpids)
        {
            Message startMessage = this.protocol.RequestDpids(dpids);
            if (!await this.SendMessage(startMessage))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Read a dpid response from the PCM.
        /// </summary>
        public async Task<RawLogData> ReadLogData()
        {
            Message message;
            RawLogData result = null;

            for (int attempt = 1; attempt < 3; attempt++)
            {
                message = await this.ReceiveMessage();
                if (message == null)
                {
                    break;
                }

                this.logger.AddDebugMessage("ReadLogData: " + message.ToString());
                
                if (this.protocol.TryParseRawLogData(message, out result))
                {
                    break;
                }
            } 

            return result;
        }

        /// <summary>
        /// Currently only used by VpwExplorer for testing.
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public async Task<Response<int>> GetPid(UInt32 pid)
        {
            Message request = this.protocol.CreatePidRequest(pid);
            if(!await this.TrySendMessage(request, "PID request"))
            {
                return Response.Create(ResponseStatus.Error, 0);
            }

            Message responseMessage = await this.ReceiveMessage();
            if (responseMessage == null)
            {
                return Response.Create(ResponseStatus.Error, 0);
            }

            return this.protocol.ParsePidResponse(responseMessage);
        }

        /// <summary>
        /// For historical reference only.
        /// </summary>
        /// <returns></returns>
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
            //message = this.protocol.BeginLogging(dpid1, dpid2);//, dpid3);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            return true;
        }
    }
}
