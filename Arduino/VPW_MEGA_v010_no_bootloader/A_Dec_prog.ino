const long Read_From_Address = 0x0; // address to start reading from in the EEPROM
const long Read_To_Address = 0x7FFFF; //suggestions 0x7FFFF the end address for 512k chip.  0xFFFFF the end address for 1M chip.
const long Read_Length = 0x1000; //read length used in incrementing,  0800= 2048 bytes/read, 0x1000 = 4096 bytes/read Maximum;

const boolean Supress_Non_BIN = false; //True means will only print .bin once read has started (also supresses CRC)
const boolean Print_CRC = true; //True means will print check sum

word Key=0xA5EF;//Must set the key before uploading right now

//****************** Above are USER config settings

const int reader_Begin_Byte = Supress_Non_BIN ? 10:0; //if Supress_Non_BIN = true then set to 10
const int reader_End_Cut = (Supress_Non_BIN||!Print_CRC) ? 3:0; //if Supress_Non_BIN = true then set to 3 to leave off CRC


long time_Pid2;



long Sending_Timer2=500;//timer for codes

int Akn = 1; //flag for if we have rec'd a response
word Seed;

String VPW_Message ="";
boolean Have_Seed;
boolean ECU_Live = 0; //ECU is sending messages
int Have_Key; //0=don't have the key yet.  1= has the key.  Stop and send message.  3= message sent. stopped.
int Read_Start = 0; //don't start requesting read until normal message
long Seed_Wait=200;//variable due to 10 sec after 2nd try
long time_Seed;

long Current_Address = 0; //address being read at the time

const byte Request_Seed[] ={0x6c,0x10,0xF0,0x27,0x01};
byte Send_key[] ={0x6c,0x10,0xF0,0x27,0x02, 0, 0}; //not a constant as we update the key in the program

const byte Request_VIN1 []={0x6C, 0x10, 0xF0, 0x3C, 0x01};
const byte Request_VIN2 []={0x6C, 0x10, 0xF0, 0x3C, 0x02};
const byte Request_VIN3 []={0x6C, 0x10, 0xF0, 0x3C, 0x03};
const byte Request_HDW_No []={0x6C, 0x10, 0xF0, 0x3C, 0x04};//HDW No.
const byte Request_Serial_No1 []={0x6C, 0x10, 0xF0, 0x3C, 0x05};
const byte Request_Serial_No2 []={0x6C, 0x10, 0xF0, 0x3C, 0x06};
const byte Request_Serial_No3 []={0x6C, 0x10, 0xF0, 0x3C, 0x07};



//upload download instructions
//const byte CMD_Tester_Present []=   {0x6C, 0xFE, 0xF0, 0x3F};
//const byte CMD_Tester_Present2 []=  {0x8C, 0xFE, 0xF0, 0x3F};

const byte CMD_Normal_Mode []=       {0x6C, 0x10, 0xF0, 0xA0};

//6C 10 F0 34 00 04 DA FF 82 00 D1 
const byte CMD_Request_Write []=     {0x6C, 0x10, 0xF0, 0x34, 0x00, 0x04, 0xDA, 0xFF, 0x82, 0x00};
const byte CMD_Request_TransferW []= {0x6C, 0x10, 0xF0, 0x36, 0x00, 0x00, 0x80, 0xFF, 0x80, 0x00};

//const byte CMD_Request_Read []=    {0x6D, 0x10, 0xF0, 0x35, 0x01, 0x10, 0x00, 0x00, 0x00, 0x00};
//6D 10 F0 35 01 10 00 00 00 00 0C 
//(eg. 128 bytes, start $0x0000){10, 0x6C, 0x10, 0xF0, 0x35, 0x01, 0x00, 0x80, 0x00, 0x00, 0x00};
//Bytes 5-9 set in setup.  7-9 indexed as reading.  Bytes 4&6 are read size.  5-9 are the address to read from
byte CMD_Request_Read []=            {0x6D, 0x10, 0xF0, 0x35, 0x01, 0x10, 0x00, 0x00, 0x00, 0x00}; 


const byte CMD_Request_Transfer []=  {0x6D, 0x10, 0xF0, 0x36, 0x80, 0x00, 0x00, 0xFF, 0x91, 0x3E, 0x02, 0x4E};
const byte CMD_Block_Transfer_OK []= {0x6C, 0x10, 0xF0, 0x76, 0x00, 0x54, 0x00, 0x00, 0x00, 0x00};

const byte CMD_Read_Block []=        {0x6C, 0x10, 0xF0, 0x3C, 0xBB};//6C 10 F0 3C BB 34 
const byte CMD_Read_Block2 []=       {0x6C, 0x10, 0xF0, 0x3C, 0x21}; //6C 10 F0 3C 21 0D 

const byte CMD_Disable_Transmission []=  {0x6C, 0xFE, 0xF0, 0x28, 0x00};



long Sending_Timer;


