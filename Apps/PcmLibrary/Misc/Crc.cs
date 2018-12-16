using System;
using System.Collections.Generic;
using System.Text;

namespace PcmHacking
{
    /// <summary>
    /// From https://barrgroup.com/Embedded-Systems/How-To/CRC-Calculation-C-Code
    /// </summary>
    public class Crc
    {
        private static UInt32[] crcTable;
        private const int WIDTH = 8 * 4;
        private const UInt32 TOPBIT = 0x80000000;
        private const UInt32 POLYNOMIAL = 0x04C11DB7;

        public Crc()
        {
            if (crcTable == null)
            {
                crcTable = new UInt32[256];
                UInt32 remainder;

                /*
                 * Compute the remainder of each possible dividend.
                 */
                for (int dividend = 0; dividend < 256; ++dividend)
                {
                    /*
                     * Start with the dividend followed by zeros.
                     */
                    remainder = (UInt32)(dividend << (WIDTH - 8));

                    /*
                     * Perform modulo-2 division, a bit at a time.
                     */
                    for (int bit = 8; bit > 0; --bit)
                    {
                        /*
                         * Try to divide the current data bit.
                         */
                        if ((remainder & TOPBIT) != 0)
                        {
                            remainder = (remainder << 1) ^ POLYNOMIAL;
                        }
                        else
                        {
                            remainder = (remainder << 1);
                        }
                    }

                    /*
                     * Store the result into the table.
                     */
                    crcTable[dividend] = remainder;
                }
            }
        }

        public UInt32 GetCrc(byte[] buffer, UInt32 start, UInt32 length)
        {
            byte data;
            UInt32 remainder = 0;

            for (UInt32 index = start; index < start + length; index++)
            {
                /*
                 * Divide the message by the polynomial, a byte at a time.
                 */
                data = (byte)(buffer[index] ^ (remainder >> (WIDTH - 8)));
                remainder = crcTable[data] ^ (remainder << 8);
            }

            /*
             * The final remainder is the CRC.
             */
            return (remainder);
        }
    }
}
