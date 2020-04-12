#include "common.h"

/* From https://barrgroup.com/Embedded-Systems/How-To/CRC-Calculation-C-Code
 *
 * The width of the CRC calculation and result.
 * Modify the typedef for a 16 or 32-bit CRC standard.
 */
typedef unsigned int crc;
typedef unsigned char uint8_t;

#define WIDTH  (8 * sizeof(crc))
#define TOPBIT (1 << (WIDTH - 1))
#define POLYNOMIAL 0x04C11DB7

crc __attribute((section(".kerneldata"))) crcTable[256];

// These are not from the original code, they're used to support background CRC computation.
uint8_t __attribute((section(".kerneldata"))) *crcStartAddress;
int __attribute((section(".kerneldata"))) crcLength;
int __attribute((section(".kerneldata"))) crcIndex;
crc __attribute((section(".kerneldata"))) crcRemainder;

void crcInit(void)
{
    crcStartAddress = 0;
    crcLength = 0;
    crcIndex = 0;
    crcRemainder = 0;

    crc  remainder;

    /*
     * Compute the remainder of each possible dividend.
     */
    for (int dividend = 0; dividend < 256; ++dividend)
    {
        ScratchWatchdog();

        /*
         * Start with the dividend followed by zeros.
         */
        remainder = dividend << (WIDTH - 8);

        /*
         * Perform modulo-2 division, a bit at a time.
         */
        for (uint8_t bit = 8; bit > 0; --bit)
        {
            /*
             * Try to divide the current data bit.
             */
            if (remainder & TOPBIT)
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

}   /* crcInit() */

// Used by the erase function to destory any previously calculated CRC
void crcReset(void)
{
  crcStartAddress = 0;
  crcLength = 0;
  crcIndex = 0;
  crcRemainder = 0;
}

crc crcFast(unsigned char *message, int nBytes)
{
    uint8_t data;
    crc remainder = 0;


    /*
     * Divide the message by the polynomial, a byte at a time.
     */
    for (int byte = 0; byte < nBytes; ++byte)
    {
        if (nBytes % 256 == 0)
        {
            ScratchWatchdog();
        }

        data = message[byte] ^ (remainder >> (WIDTH - 8));
        remainder = crcTable[data] ^ (remainder << 8);
    }

    /*
     * The final remainder is the CRC.
     */
    return (remainder);

}   /* crcFast() */

///////////////////////////////////////////////////////////////////////////////

int crcIsStarted(unsigned char *message, int nBytes)
{
    if (crcStartAddress != message)
    {
        return 0;
    }

    if (crcLength != nBytes)
    {
        return 0;
    }

    return 1;
}

int crcIsDone(unsigned char *message, int nBytes)
{
    if (!crcIsStarted(message, nBytes))
    {
        return 0;
    }

    if (crcIndex == nBytes)
    {
        return 1;
    }

    return 0;
}

void crcStart(unsigned char *message, int nBytes)
{
    crcStartAddress = message;
    crcLength = nBytes;
    crcIndex = 0;
    crcRemainder = 0;
}

crc crcGetResult()
{
    return crcRemainder;
}

void crcProcessSlice()
{
    if (crcLength == 0)
    {
        return;
    }

    int limit = crcLength;
    int chunkSize = 8192;
    if ((crcIndex + chunkSize) < limit)
    {
        limit = crcIndex + chunkSize;
    }

    uint8_t data;
    for( ; crcIndex < limit; crcIndex++)
    {
        if (crcIndex % 256 == 0)
        {
            ScratchWatchdog();
        }
        data = crcStartAddress[crcIndex] ^ (crcRemainder >> (WIDTH - 8));
        crcRemainder = crcTable[data] ^ (crcRemainder << 8);
    }
}
