void setup()
{
#define _DEBUG
#define VPW_DEBUG
//set up initial read command

 CMD_Request_Read[5]=highByte(Read_Length); //Set the Request_Read message to have the Read_Length Specified (1,000 bytes, 4000 bytes 
 CMD_Request_Read[6]=lowByte(Read_Length);
 CMD_Request_Read[7]=(Read_From_Address>>16)&0xFF; //high byte
 CMD_Request_Read[8]=(Read_From_Address>>8)&0xFF; //mid byte
 CMD_Request_Read[9]=(Read_From_Address)&0xFF; //low byte
  
  
  //pinMode(2,INPUT_PULLUP);
  pinMode(SS,OUTPUT);
  pinMode(10,OUTPUT);
  pinMode(13,OUTPUT);
  digitalWrite(13,LOW);
  pinMode(A5,OUTPUT);
  digitalWrite(A5,HIGH);

writer_init();
  reader_init();
  // enable all interrupts
  //sei();
 

  //set up serial port for debugging with data to PC screen
  Serial.begin(115200);  
  Serial.println(VERSION);
 

  VPW_del=4;

  Serial.println("press any key to continue");
  while(!Serial.available())
  {}
  Serial.read();
  
}

void loop()
{ //send_pid_VPW(mode,pid)

 //************************************* tester present constant send ***************************************
 //likely this is not needed when reading.   Will test and find out 
 /*
 if(Have_Key > 0 && millis()-Sending_Timer>4000&&Have_Key<9){ //sending timer
 //send_VPW(CMD_Tester_Present,sizeof(CMD_Tester_Present));
  Sending_Timer=millis();
}
*/
//*******************************************  Sending comands *************************************************
if(Akn == 1&&millis()-Sending_Timer2>1000){ //sending timer millis()-Sending_Timer2>2500
 Akn =0; //clear fla
 if (Have_Key == 1){send_VPW(CMD_Disable_Transmission,sizeof(CMD_Disable_Transmission));} //CMD_Normal_Mode,sizeof(CMD_Normal_Mode)  CMD_Disable_Transmission
 else if (Have_Key == 2){send_VPW(CMD_Request_Write,sizeof(CMD_Request_Write));} 
 // load boot loader
 else if (Have_Key == 3){send_VPW_With_CRC_P(CMD_Boot1,sizeof(CMD_Boot1));Serial.println("boot1 send");} // CMD_Read_Block2 CMD_Request_Read // send_VPW_LONG_P // FOR PROGRAM MEMORY STORED COMMANDS
 else if (Have_Key == 4){send_VPW_With_CRC_P(CMD_Boot2,sizeof(CMD_Boot2));Serial.println("boot2 send");}  //CMD_Request_Read CMD_Disable_Transmission  CMD_Request_Transfer  CMD_Request_Read CMD_Request_Write
 else if (Have_Key == 5){send_VPW_With_CRC_P(CMD_Boot3,sizeof(CMD_Boot3));Serial.println("boot3 send");} 
 
 else if (Have_Key == 6){send_VPW(CMD_Read_Block,sizeof(CMD_Read_Block));}  //CMD_Read_Block; not sure what this does
 else if (Have_Key == 7){send_VPW(CMD_Read_Block2,sizeof(CMD_Read_Block2));}  //CMD_Read_Block2; not sure what this does
 
 //++++++++++++++++++++  Start reading memory segments +++++++++++++++++++++
 else if (Have_Key == 8)
 {
   if(Current_Address==Read_From_Address)Serial.println("reading bin data.  Wait.....");
    //see if the packet size needs to be changed
    long End_Address = Current_Address+Read_Length; //calculate the end address of this read.
    if(End_Address>Read_To_Address){ //if the end of our read is past end of the needed data
         long Read_Length_New = (Read_To_Address - Current_Address)+1; //addjust read length for the end
         CMD_Request_Read[5]=highByte(Read_Length_New); //Set the Request_Read message to have the Read_Length Specified (1,000 bytes, 4000 bytes 
         CMD_Request_Read[6]=lowByte(Read_Length_New);
         End_Address = Current_Address+Read_Length_New;
          }
   send_VPW(CMD_Request_Read,sizeof(CMD_Request_Read));
   //Serial.print("address: ");Serial.println(Current_Address,HEX); 
   //CMD_Request_Read
   //setup for NEXT read.
     Current_Address += Read_Length;  //update address to read from
     if(Current_Address>Read_To_Address) {Have_Key=9;} //if exceeding read address set to 10 to jump out.Serial.println("exiting read");
     else{
       CMD_Request_Read[7]=(Current_Address>>16)&0xFF; //high byte
       CMD_Request_Read[8]=(Current_Address>>8)&0xFF; //mid byte
       CMD_Request_Read[9]=(Current_Address)&0xFF; //low byte
        }

  // else Have_Key--; //reduce by one to keep us at 8 until we get all the data read. (go over the address end address)
 } 
 else if (Have_Key == 10){Serial.println();Serial.print("************ Finished. Read:");Serial.print(Read_To_Address+1);Serial.print(" bytes.  Finished address: ");Serial.println(Read_To_Address,HEX);
       send_VPW(CMD_Normal_Mode,sizeof(CMD_Normal_Mode));}  //CMD_Request_Read //CMD_Normal_Mode
 Have_Key++; 
Sending_Timer2=millis();

}
// _____________________________________ end of sending commands ________________________________________

  if (Have_Seed == 1 && ECU_Live == 1&& millis() - time_Seed > Seed_Wait) //chenged Have_seed from = 0 to =1 to prevent code from trying to read with out bootloaders in program
  {
    time_Seed=millis();
    //message_counter++;
    send_VPW(Request_Seed,sizeof(Request_Seed)); //send command
    Seed_Wait = 10000;
  }



  if(packet_waiting) // if there was a message and it was recieved propperly
  {
    packet_waiting = 0;
    if(packet_size>15) //Only print the large messages.  Small messages are printed using Print_MSG() other places
    {
     
      int size = packet_size-reader_End_Cut;
      Serial.println(); //blank line to seperate the .bin reads
      for(int x=reader_Begin_Byte;x<size;x++) //loop to output the message
      {
        byte b;
        if(x<15)b=packet_data[x];//completed message for small messages
        else    b=reader_data[x];//large data moved over to reader data so it's not overwritten by next message.
       // if(x<0||x>=size)continue; //  the first 10 are the command.  Last 3 are the check sum.  To skip use if(x<10||x>=size-3)
        if(b<=0x0F) Serial.print("0"); //if a single character add a leading zero.
        Serial.print(b,HEX); //print the data to the screen
        Serial.print(" ");
        if((x+1-reader_Begin_Byte)%16==0)Serial.println();
      }
      Serial.println();
      Serial.println();
    }
    else
    {
    memcpy(VPW_data,(const void*)packet_data,15); //move data into the VPW data for use in other parts of the program
    VPW_len = packet_size;

   ECU_Live = 1; //flag to note ECU is sending messages
   VPW_Message =""; //clear message

     //Store message into VPW_Message string
      for(int x=0;x<VPW_len-1;x++) //(-1) to remove CRC
      {
        if(VPW_data[x]<=0x0F) VPW_Message += "0";
      VPW_Message += String(VPW_data[x],HEX);
      }
       VPW_Message.toUpperCase();
       
  //    Print_MSG();
if(Have_Key < 11) VPW_Decode();
//  if(have_Seed==0)VPW_Decode(); //use long messages until we get the seed key going
//  else   VPW_Decode2(); //if we are into the writing sequence then use decode2 (less printing)
    VPW_Message="0 0 0 0"; //clear so doesn't reprint
    }
  }//end if message recieved
  
}// End of Main Void



//***************************************************************************

void Print_MSG()
{
      for(int x=0;x<VPW_len-!Print_CRC;x++) //(-1) to remove CRC
      {
        if(VPW_data[x]<=0x0F) Serial.print("0"); //if a single character add a leading zero.
        Serial.print(VPW_data[x],HEX);
       Serial.print(" ");
      }
      Serial.println(" ");
    
}

void Print_MSG_Nln()
{
      for(int x=0;x<VPW_len-!Print_CRC;x++) //(-1) to remove CRC
      {
       if(VPW_data[x]<=0x0F) Serial.print("0"); //if a single character add a leading zero.
       Serial.print(VPW_data[x],HEX);
       Serial.print(" ");
      }
   
}

/*
void send_VPW(byte* msg,byte len)
 {
 
 digitalWrite (CS_VPW, LOW);
 SPI.transfer (CMD_SEND_VPW_MSG);
 for (int i=0; i<len; i++)
 {
 delayMicroseconds(VPW_DEL);
 SPI.transfer(msg[i]);
 }
 digitalWrite (CS_VPW, HIGH);
 
 }
 
 */





