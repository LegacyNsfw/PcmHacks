#include <stdio.h>
//#include "common.h"

extern int  crcTable[256];

void __cdecl crcInit(void);
unsigned __cdecl crcFast(unsigned char *message, int nBytes);
void __cdecl ScratchWatchdog();

int main()
{
    crcInit();

    // This shows that the table is initialized.
    //for (int i = 0; i < 256; i++)
    //{
    //    printf("%08X\r\n", crcTable[i]);
    //}

    FILE *file = fopen("FirstReadFromCorvette.bin", "r");
    unsigned char* content = new unsigned char[512*1024];
    int size = fread(content, 8, 512*1024, file);
    printf("Read %d bytes\r\n", size);

    
    unsigned crc = crcFast(content, 0x2000);
    printf ("CRC: %08x\r\n", crc);
}

void ScratchWatchdog()
{

}