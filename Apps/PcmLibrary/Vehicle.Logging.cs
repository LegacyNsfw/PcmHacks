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
            // Configure logging parameters

            // Load SAE RPM to DPID FE positions 1 and 2
            Message message = this.protocol.ConfigureDynamicData(0xFE, DpidPosition.Position12, 0x000C);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 2 bytes from SAE MAF to positions 3 and 4
            message = this.protocol.ConfigureDynamicData(0xFE, DpidPosition.Position34, 0x0010);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of SAE TPS to position 5
            message = this.protocol.ConfigureDynamicData(0xFE, DpidPosition.Position5, 0x0011);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // load 1 byte of SAE TPS to position 5
            message = this.protocol.ConfigureDynamicData(0xFE, DpidPosition.Position6, 0x000B);
            if (!await this.SendMessage(message))
            {
                return false;
            }

            // Start logging
            message = this.protocol.BeginLogging(0xFE);
            if (!await this.SendMessage(message))
            {
                return false;
            }
        }

        /// <summary>
        /// Query the PCM's Serial Number.
        /// </summary>
        public async Task<string[]> ReadLogRow()
        {
            Message message = await this.ReceiveMessage();

        }
    }
}
