using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// It is tempting to make this an enum, but if we need to change
    /// the Tool ID at run-time that will become a problem.
    /// </summary>
    class DeviceId
    {
        /// <summary>
        /// OBD2 Device ID for the Powertrain Control Module.
        /// </summary>
        public const byte Pcm = 0x10;

        /// <summary>
        /// OBD2 Device ID for the
        /// </summary>
        public const byte Tool = 0xF0;

        /// <summary>
        /// OBD2 Device ID for a broadcast message
        /// </summary>
        public const byte Broadcast = 0xFE;
    }
}
