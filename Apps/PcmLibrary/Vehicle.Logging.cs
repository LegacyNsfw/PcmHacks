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
    /// <remarks>
    /// Methods in this class are high-level operations like "get the VIN," or "read the contents of the EEPROM."
    /// </remarks>
    public partial class Vehicle : IDisposable
    {
        /// <summary>
        /// Start logging
        /// </summary>
        public async Task<bool> StartLogging()
        {
            // NOT SUPPORTED (in my ROM anyway, need to try others)
            // 19F3 - transmission temprature
            // 1602 - oil temperature
            // 125D - knock retard
            // 19F5 - current gear
            // 125E - knock count, two-byte

            // Configure logging parameters
            // 0xF2-0xFF all work
            byte dpidF2 = 0xF2;


            // Load 2 bytes of SAE RPM to DPID positions 1 and 2
            Message message = this.protocol.ConfigureDynamicData(dpidF2, DpidPosition.Position12, 0x000C);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 2 bytes from SAE MAF to positions 3 and 4
            message = this.protocol.ConfigureDynamicData(dpidF2, DpidPosition.Position34, 0x0010);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of SAE MAP to position 5
            message = this.protocol.ConfigureDynamicData(dpidF2, DpidPosition.Position5, 0x000B);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of SAE TPS to position 6
            message = this.protocol.ConfigureDynamicData(dpidF2, DpidPosition.Position6, 0x0011);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            byte dpidF3 = 0xF3;

            // Load SAE IAT to DPID position 1
            message = this.protocol.ConfigureDynamicData(dpidF3, DpidPosition.Position1, 0x000F);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // Load SAE coolant temperature to position 2
            message = this.protocol.ConfigureDynamicData(dpidF3, DpidPosition.Position2, 0x0005);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of SAE trans temp to position 3 (this is raw sensor value.)
            message = this.protocol.ConfigureDynamicData(dpidF3, DpidPosition.Position3, 0x19AD);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of SAE speed (KMH) to position 4 
            message = this.protocol.ConfigureDynamicData(dpidF3, DpidPosition.Position4, 0x000D);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of GM Knock to position 5
            message = this.protocol.ConfigureDynamicData(dpidF3, DpidPosition.Position5, 0x11A6);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of GM fuel status to position 6 
            message = this.protocol.ConfigureDynamicData(dpidF3, DpidPosition.Position6, 0x1105);
            if (!await this.SendMessage(message))
            {
                return false;
            }


            byte dpidF4 = 0xF4;

            // Load left LTFT to position 1
            message = this.protocol.ConfigureDynamicData(dpidF4, DpidPosition.Position1, 0x0007);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // Load right LTFT to position 2
            message = this.protocol.ConfigureDynamicData(dpidF4, DpidPosition.Position2, 0x0009);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of Gm Target AFR to position 3 (0:1-25.5:1)
            message = this.protocol.ConfigureDynamicData(dpidF4, DpidPosition.Position3, 0x119E);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of GM battery voltage to position 4
            message = this.protocol.ConfigureDynamicData(dpidF4, DpidPosition.Position4, 0x1141);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of GM current BLM cell to position 5 
            message = this.protocol.ConfigureDynamicData(dpidF4, DpidPosition.Position5, 0x1190);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of "normalized TPS" (so what's the other TPS parmeter?) to position 6
            message = this.protocol.ConfigureDynamicData(dpidF4, DpidPosition.Position6, 0x1151);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // Start logging
            message = this.protocol.BeginLogging(dpidF2, dpidF3, dpidF4);
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
            return null;
        }
    }
}
