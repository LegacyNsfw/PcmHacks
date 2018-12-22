//#include <stdio.h>
//#include "common.h"
#include <iostream>
#include <fstream>
using namespace std;

extern int  crcTable[256];

void __cdecl crcInit(void);
unsigned __cdecl crcFast(unsigned char *message, int nBytes);
void ScratchWatchdog();

int  main()
{
    crcInit();

    // This shows that the table is initialized.
    //for (int i = 0; i < 256; i++)
    //{
    //    printf("%08X\r\n", crcTable[i]);
    //}

    //FILE *file = fopen("FirstReadFromCorvette.bin", "r");
    ifstream file ("FirstReadFromCorvette.bin", ios::in|ios::binary|ios::ate);
    streampos size = file.tellg();
    char* content = new char[512*1024];
    file.read(content, 512*1024);
    file.close();
    printf("Read %d bytes\r\n", size);

    
    unsigned crc = crcFast((unsigned char*)content, 0x2000);
    printf ("CRC: %08x\r\n", crc);

    delete content;
    return 0;
}

void ScratchWatchdog()
{

}