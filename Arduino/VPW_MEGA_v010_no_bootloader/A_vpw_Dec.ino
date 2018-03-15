byte VPW_data[15];
byte VPW_len;
long time_VPW;
int VPW_del;

#define CS_VPW A3
#define VPW_RST 10

#define CMD_VPW_GET_MSG 0x56
#define CMD_SEND_VPW_MSG    0x54
#define CMD_VPW_AVAILABLE    0x53 //packet_waiting | (oversizedRecieved << 1) | (oversizedColision << 2) | (oversizedFinishedOut << 3) | (writer_started << 4) | (noSpace << 5)|(readerOutOfMemory<<6);
#define VPW_SPI_ERROR    0xEF
#define VPW_SPI_ACKNOWLEDGE    0xA0
#define VPW_SPI_VERSION    0x64
#define CMD_VPW_GET_MSG_OVERSIZED 0x58
#define CMD_VPW_SEND_MSG_NO_CRC 0x45
#define CMD_VPW_SEND_MSG_NO_CRC_LONG 0x47


#define VPW_DEL 50
int message_counter;


bool readingOversideVPW=false;


