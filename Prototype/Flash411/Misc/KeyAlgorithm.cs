using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    class KeyAlgorithm
    {
        /// <summary>
        /// Gets the unlock value for the given key.
        /// </summary>
        /// <remarks>
        /// The code in the GitHub 'issue' discussion recommended byte-swapping, but 
        /// it only worked for me if I don't swap the byte order. Perhaps because bytes
        /// get swapped when parsing the GetSeed results.
        /// </remarks>
        public static UInt16 GetKey(UInt16 seed)
        {
            UInt16 key = 0x934D;
            key -= seed;
            return key;
        }
    }
}
