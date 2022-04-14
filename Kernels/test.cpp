//#include <stdio.h>
//#include "common.h"
#include <iostream>
#include <fstream>

typedef unsigned       uint32_t;

using namespace std;

extern int crcTable[256];

void __cdecl crcInit(void);
unsigned __cdecl crcFast(unsigned char *message, int nBytes);
void ScratchWatchdog();

int  main()
{
    crcInit();

    ifstream file ("FirstReadFromCorvette.bin", ios::in|ios::binary|ios::ate);
    streampos size = file.tellg();
	cout << "Size: " << size << endl;
    char* content = new char[512*1024];
    file.seekg(streampos(0), ios::beg);
    file.read(content, 512*1024);
    file.close();
	
    printf("Read %d bytes\r\n", (unsigned)file.gcount());

	uint32_t * pData = (uint32_t*)content;
	//printf("Pointer: %08X\r\n", (uint32_t)pData);
	for (int i = 0; i < 10; i++)
	{
		printf("%02d %08X %08X\r\n", i, crcTable[i], pData[i]);
	}

    unsigned crc = crcFast((unsigned char*)content, 0x2000);
    printf ("CRC: %08x\r\n", crc);

    delete content;
    return 0;
}

void ScratchWatchdog()
{

}

