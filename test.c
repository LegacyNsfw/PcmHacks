#include <stdio.h>
#include "common.h"

extern int crcTable[256];

int main()
{
    crcInit();
    for (int i = 0; i < 256; i++)
    {
        printf("%08X", crcTable[i]);
    }
}