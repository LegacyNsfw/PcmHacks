///////////////////////////////////////////////////////////////////////////////
// Functions for erasing and writing flash
///////////////////////////////////////////////////////////////////////////////
#if defined P12
	#define SIM_BASE        0x00FFFA30
	#define SIM_20          (*(unsigned short *)(SIM_BASE + 0x20)) // Lock functions
#else
	#define SIM_BASE        0x00FFFA00
#endif
#define SIM_CSBARBT     (*(unsigned short *)(SIM_BASE + 0x48)) // CSRBASEREG, boot chip select, chip select base addr boot ROM reg,
															   // must be updated to $0006 on each update of flash CE/WE states
#define SIM_CSORBT      (*(unsigned short *)(SIM_BASE + 0x4a)) // FFFA7A (not used) CSROPREG, Chip select option boot ROM reg., $6820 for normal op
#define SIM_CSBAR0      (*(unsigned short *)(SIM_BASE + 0x4c)) // FFFA7C CSBASEREG, chip selects
#define SIM_CSOR0       (*(unsigned short *)(SIM_BASE + 0x4e)) // FFFA7E *Chip select option reg., $1060 for normal op, $7060 for accessing flash chip
#define HARDWARE_IO     (*(unsigned short *)(0xFFFFE2FA))      // ?????? Hardware I/O reg

#define FLASH_BASE         (*(unsigned short *)(0x00000000))
#define FLASH_IDENTIFIER   (*(uint32_t *)(0x00000000))
#define FLASH_MANUFACTURER (*(uint16_t *)(0x00000000))
#define FLASH_DEVICE       (*(uint16_t *)(0x00000002))

#define SIGNATURE_COMMAND  0x9090
#define READ_ARRAY_COMMAND 0xFFFF

/*char volatile * const  SIM_MCR      =   SIM_BASE + 0x00; // Module Control register
char volatile * const  SIM_SYNCR    =   SIM_BASE + 0x04; // Clock synthesiser control register
char volatile * const  SIM_RSR      =   SIM_BASE + 0x07; // Reset Status
char volatile * const  SIM_SYPCR    =   SIM_BASE + 0x21; // System Protection
char volatile * const  SIM_PICR     =   SIM_BASE + 0x22; // Periodic Timer
char volatile * const  SIM_PITR     =   SIM_BASE + 0x24; //
char volatile * const  SIM_????     =   SIM_BASE + 0x25; // NEW AND USED FOR P12
char volatile * const  SIM_SWSR     =   SIM_BASE + 0x27; //
char volatile * const  SIM_CSPAR0   =   SIM_BASE + 0x44; // chip select pin assignment
char volatile * const  SIM_CSPAR1   =   SIM_BASE + 0x46; //

char volatile * const  SIM_CSBAR1   =   SIM_BASE + 0x50;
char volatile * const  SIM_CSOR1    =   SIM_BASE + 0x52;
char volatile * const  SIM_CSBAR2   =   SIM_BASE + 0x54;
char volatile * const  SIM_CSOR2    =   SIM_BASE + 0x56;
char volatile * const  SIM_CSBAR3   =   SIM_BASE + 0x58;
char volatile * const  SIM_CSOR3    =   SIM_BASE + 0x5a;
char volatile * const  SIM_CSBAR4   =   SIM_BASE + 0x5c;
char volatile * const  SIM_CSOR4    =   SIM_BASE + 0x5e;
char volatile * const  SIM_CSBAR5   =   SIM_BASE + 0x60;
char volatile * const  SIM_CSOR5    =   SIM_BASE + 0x62;
char volatile * const  SIM_CSBAR6   =   SIM_BASE + 0x64;
char volatile * const  SIM_CSOR6    =   SIM_BASE + 0x66;*/

// Lock and unlock refers to the internal +12v supply to the flash chip, needed for erasing and writing.
// true to unlock, false to lock.
void FlashUnlock(bool unlock);

// Functions prefixed with Intel work with this chip ID
#define FLASH_ID_INTEL_28F400B 0x00894471 // 512k
#define FLASH_ID_INTEL_28F800B 0x0089889D // 1m

uint32_t Intel_GetFlashId();
uint8_t Intel_EraseBlock(uint32_t address);
uint8_t Intel_WriteToFlash(unsigned int payloadLengthInBytes, unsigned int startAddress, unsigned char *payloadBytes, int testWrite);

// Functions prefixed with Amd work with this chip ID
#define FLASH_ID_AMD_AM29F800BB 0x00012258 // 1m
#define FLASH_ID_AMD_AM29BL162C 0x00012203 // 2m
#define FLASH_ID_AMD_AM29BL802C 0x00012281 // 1m

uint32_t Amd_GetFlashId();
uint8_t Amd_EraseBlock(uint32_t address);
uint8_t Amd_WriteToFlash(unsigned int payloadLengthInBytes, unsigned int startAddress, unsigned char *payloadBytes, int testWrite);
