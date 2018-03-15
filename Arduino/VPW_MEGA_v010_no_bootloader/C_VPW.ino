void send_pid_VPW(byte id)
{
 
  byte msg[5];
  msg[0] = 0x68;
  msg[1] = 0x6a;
  msg[2] = 0xf0;
  msg[3] = 0x01;
  msg[4] = id;
send_VPW(msg,5);

}
void send_VPW(byte* msg,word len)
{
if(len>15)writer_send(msg,len,true,false); //if its longer than buffer will just send it and hope it doesnt change partway
else 
{
  if(writer_started&&writer_buf==writer_data)return; //dont mess up current send
  memcpy(writer_buf,msg,len);
  writer_send(writer_buf,len,true,false);
}
}
void send_VPW(const byte* msg,word len) //its constant so dont bother puting it in a buffer
{
    writer_send(msg,len,true,false);
}
void send_VPW_P(const byte* msg,word len) //for use with Program memory IE: PROGMEM
{
    writer_send(msg,len,true,true);
}
void send_VPW_With_CRC(byte* msg,word len)
{
  if(len>15)writer_send(msg,len,false,false); //if its longer than buffer will just send it and hope it doesnt change partway through send
else 
{
  if(writer_started&&writer_buf==writer_data)return; //dont mess up current send
  memcpy(writer_buf,msg,len); //if it small enough copy into the hard buffer so the other data can be changed if necesarry
  writer_send(writer_buf,len,false,false);
}
}
void send_VPW_With_CRC(const byte* msg,word len) // sending with crc a const 
{
  writer_send(msg,len,false,false);
}
void send_VPW_With_CRC_P(const byte* msg,word len)
{
  writer_send(msg,len,false,true);
}




