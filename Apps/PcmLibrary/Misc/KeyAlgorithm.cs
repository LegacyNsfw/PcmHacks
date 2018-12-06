using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    public class KeyAlgorithm
    {
        /// <summary>
        /// Gets the unlock value for the given key.
        /// </summary>
        /// <remarks>
        /// The code in the GitHub 'issue' discussion recommended byte-swapping, but 
        /// it only worked for me if I don't swap the byte order. Perhaps because bytes
        /// get swapped when parsing the GetSeed results.
        /// </remarks>
        public static UInt16 GetKey(int algo, UInt16 seed)
        {
            UInt16 key;

            switch (algo)
            {
                case 01:    key = GetKey_01(seed);
                            break;
                case 02:    key = GetKey_02(seed);
                            break;
                default:    key=0x0000;
                            break;
            }
            return key;
        }

#region algos
        // Algorithm 1 ('0411)
        public static UInt16 GetKey_01(UInt16 seed)
        {
            UInt16 key = 0x934D;
            key -= seed;
            return key;
        }

        // Algorithm 2 (not implemented)
        public static UInt16 GetKey_02(UInt16 seed)
        {
            UInt16 key = 0x0000;

            return key;
        }
#endregion

        // Perhaps these should be moved to utility?
#region bitshifting
        public static int swapab(int bytes)
        {
            int lo = bytes >> 8;
            int hi = bytes & 0xFF;
            return (ushort)(hi << 8 + lo);
        }

        public static int RotateLeft(UInt16 bytes, byte count)
        {
            return (bytes << count | bytes >> (0x10 - count));
        }

        public static int RotateRight(UInt16 bytes, byte count)
        {
            return (bytes >> count| bytes << (0x10 - count));
        }
#endregion

    }
}
