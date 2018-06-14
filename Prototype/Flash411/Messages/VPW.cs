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
        /// 0x48: 6=0100=Priority 1, GM. 8=1000 GM, Functional, Type 0
        /// </summary>
        public const byte Functional0Low = 0x48;

        /// <summary>
        /// 0x68: 6=0110=Priority 3, GM. 8=1000 GM, Functional, Type 0
        /// </summary>
        public const byte Functional0 = 0x68;
        
        /// <summary>
        /// 0x6D: 0110=Priority 3, GM. C=1101 GM, Functional, Type 1 (Block Transfer)
        /// </summary>
        public const byte Block = 0x6D;

        /// <summary>
        /// 0x6A: 0110=Priority 3, GM. C=1010 GM, Functional, Type 2
        /// </summary>
        public const byte Functional2 = 0x6A;

        /// <summary>
        /// 0x6C: 0110=Priority 3, GM. C=1100 GM, Physical, Type 0
        /// </summary>
        public const byte Physical0 = 0x6C;

        /// <summary>
        /// 0x8C: 1000=Priority 4, GM. C=1100 GM, Physical, Type 0
        /// </summary>
        public const byte Physical0High = 0x8C;
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

    class VPWUtils
    {
        /// <summary>
        /// Write a 16 bit sum to the end of a block, returns a Message, as a byte array
        /// </summary>
        /// <remarks>
        /// Caller to provide valid array
        /// </remarks>
        public UInt16 CalcBlockChecksum(byte[] Block)
        {
            UInt16 Sum = 0;
            int PayloadLength = (Block[5] << 8) + Block[6];

            for (int i = 4; i < PayloadLength; i++) // skip prio, dest, src, mode
            {
                Sum += Block[i];
            }

            return Sum;
        }

        /// <summary>
        /// Write a 16 bit sum to the end of a block, returns a Message, as a byte array
        /// </summary>
        /// <remarks>
        /// Appends 2 bytes at the end of the array with the sum
        /// TODO: Throw an error if the input data is not valid?
        /// 
        /// 6C|10|F0|36/80|03 F1|FF 91 50 .... CA CS
        /// 0  1  2  3  4  5  6  7  8  9
        /// 1  2  3  4  5  6  7  8  9  10      11 12
        /// </remarks>
        public byte[] AddBlockChecksum(byte[] Block)
        {
            UInt16 Sum = 0;
            int PayloadLength;

            // Only generate the sum and append to the block if the length is right
            if (Block.Length > 6) // Do we have a length?
            {
                PayloadLength = (Block[5] << 8) + Block[6];
                if (Block.Length == PayloadLength + 12) // Correct block size?
                {
                    Sum = CalcBlockChecksum(Block);
                    byte[] BlockSum = new byte[PayloadLength + 12];

                    BlockSum[BlockSum.Length - 2] = unchecked((byte)(Sum >> 8));
                    BlockSum[BlockSum.Length - 1] = unchecked((byte)(Sum & 0xFF));

                    return BlockSum;
                }
            }
            return Block;
        }
    }
}
