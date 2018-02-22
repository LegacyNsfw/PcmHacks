using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// This class creates and parses raw VPW messages.
    /// </summary>
    public static class Protocol
    {
        /// <summary>
        /// Returns the sequence of bytes to query the VIN from the PCM.
        /// </summary>
        public static byte[] CreateVinQuery()
        {
            return new byte[] { 0, 0 };
        }

        /// <summary>
        /// Returns the sequence of bytes to query the OS from the PCM.
        /// </summary>
        public static byte[] CreateOsQuery()
        {
            return new byte[] { 0, 0 };
        }

        /// <summary>
        /// Returns the sequence of bytes to query the seed from the PCM.
        /// </summary>
        public static byte[] CreateSeedQuery()
        {
            return new byte[] { 0, 0 };
        }

        /// <summary>
        /// Gets the key value for the given seed.
        /// </summary>
        /// <remarks>
        /// This may not really belong in the Protocol class.
        /// </remarks>
        public static int GetKey(int seed)
        {
            return 0;
        }

        /// <summary>
        /// Returns the sequence of bytes to send a key to the PCM.
        /// </summary>
        public static byte[] CreateKeyResponse(int key)
        {
            return new byte[] { 0, 0 };
        }

        /// <summary>
        /// Returns the sequence of bytes to upload the kernel to the PCM.
        /// </summary>
        public static byte[] CreateKernelUpload()
        {
            return new byte[] { 0, 0 };
        }
    }
}
