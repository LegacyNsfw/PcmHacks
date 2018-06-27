                             
//First Read Boot loader Command
const byte PROGMEM CMD_Boot1[] = {0x6D, 0x10, 0xF0, 0x36, 0x00, };
//Second Read Boot Loader Command
const byte PROGMEM CMD_Boot2[] = {0x6D, 0x10, 0xF0, 0x36, 0x00, 0x02, };

//Third and final Read Boot Loader Command
const byte PROGMEM CMD_Boot3[] = {0x6D, 0x10, 0xF0, 0x36, 0x80, 0x02, 0x00, };

