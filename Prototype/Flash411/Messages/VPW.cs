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
        /// OBD2 Device ID for the tool
        /// </summary>
        public const byte Tool = 0xF0;

        /// <summary>
        /// OBD2 Device ID for a broadcast message
        /// </summary>
        public const byte Broadcast = 0xFE;
    }

    /// <summary>
    /// Defines various priority byte combinations and puts a name to them
    /// </summary>
    class Priority
    {
        /* based on information from http://www.fastfieros.com/tech/vpw_communication_protocol.htm
        Bits 7,6 and 5 are priority 0=High, 7=Low
        Bit 4 is header style (0=3 byte header-GM, 1=1 byte header-??)

        Bit 3 is In Frame Response (0=Ford, 1=GM)
        Bit 2 is addressing mode (1=Physical, 0=Functional)
        Bit 1,0 is message type: (depending on bit 2 and 3 see below)
        */

        /// <summary>
        /// 0x68 where 0110=Priority 3 with GM header and C=1000 GM, Functional, Type 0
        /// </summary>
        public const byte Type0 = 0x68;

        /// <summary>
        /// 0x6C where 0110=Priority 3 with GM header and C=1010 GM, Functional, Type 2
        /// </summary>
        public const byte Type2 = 0x6C;

        /// <summary>
        /// /// 0x6D where 0110=Priority 3 with GM header and C=1101 GM Functional, Block Transfer, Type 1
        /// </summary>
        public const byte Block = 0x6D;
    }

    /// <summary>
    /// Defines Modes
    /// </summary>
    class Mode
    {
        public const byte Response = 0x40; // added to the Mode by the PCM for it's the response

        public const byte ClearDTCs = 0x04;
        public const byte ExitKernel = 0x20;
        public const byte Seed = 0x27;
        public const byte SilenceBus = 0x28;
        public const byte ReadBlock = 0x3C;
        public const byte PCMUploadRequest = 0x34;
        public const byte PCMUpload = 0x36;
        public const byte TestDevicePresent = 0x3F;
        public const byte HighSpeedPrepare = 0xA0;
        public const byte HighSpeed = 0xA1;
    }
    class SubMode
    {
        public const byte Null = 0x00;

        public const byte GetSeed = 0x01;
        public const byte SendKey = 0x02;

        public const byte NoExecute = 0x00;
        public const byte Execute = 0x80;

        public const byte UploadOK = 0x00;

    }
}
