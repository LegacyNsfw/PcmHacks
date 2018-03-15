void send_break() {
  // hold outputPIN high for 800us
  *out_TX |= bit_TX;
  delayMicroseconds(800);
  *out_TX &= ~bit_TX;
}

void writer_send(const unsigned char *buf, uint16_t len,boolean calcCRCFlag,boolean progMem) {
  // block out receiver
  if(writer_started)return; //if already sending forget it
  writer_started = 1;

  writer_size = len; // plus one is for checksum
  writer_data = (unsigned char*)buf;

 
   //Serial.write(writer_data[1]);
   //   Serial.println(" ");
  // add the checksum
  calcCRC = calcCRCFlag;
  writeProgMem = progMem;
  if(calcCRCFlag)
  {
    writerCRC = crc8buf(buf, len);
  }
  writer_counter = 0; //clear counters and flags
  writer_bit_counter = 0;
  writer_collision = 0;
  interrupts(); //dont want to lock up program
  // wait for reader to finish
  while(reader_started) {
//digitalWrite(LED_BUILTIN,HIGH);
}
 //     digitalWrite(LED_BUILTIN,LOW);
 //if(reader_started){
 //  writer_started = 0;
  // return;
//   resend_flag=1;
 //  digitalWrite(LED_BUILTIN,HIGH);
 //  delay(100);
 //  return;
 //}
// digitalWrite(LED_BUILTIN,LOW);
 //send_break();
//Serial.println("started WRITing");
  // set SOF period, start timer, and enable its interrupt
  OCR2A = 0xFF; // start of frame time
  writer_extra = T_200US_2;
  *out_TX |= bit_TX; //high pin
  TCNT2 = 0;                  //clear timer
  TIFR2 |= (1<<OCF2A);	      // clear interrupt flag in case it was already set
  TIMSK2 |= (1<<OCIE2A);       // enable interupt
  TCCR2B |= (1<<CS21); //CLK/8 //start timer with precaler of 8
  //Serial.println(TCCR3B,BIN);
  

}
void reSend()
{
   if(writer_started)return; //if already sending forget it
  writer_started = 1;
  writer_counter = 0; //clear counters and flags
  writer_bit_counter = 0;
  writer_collision = 0;

  // wait for reader to finish
  while(reader_started) {
//digitalWrite(LED_BUILTIN,HIGH);
}
 //     digitalWrite(LED_BUILTIN,LOW);
 //if(reader_started){
 //  writer_started = 0;
  // return;
//   resend_flag=1;
 //  digitalWrite(LED_BUILTIN,HIGH);
 //  delay(100);
 //  return;
 //}
// digitalWrite(LED_BUILTIN,LOW);
 //send_break();
//Serial.println("started WRITing");
  // set SOF period, start timer, and enable its interrupt
  OCR2A = 0xFF; // start of frame time
  writer_extra = T_200US_2;
  *out_TX |= bit_TX; //high pin
  TCNT2 = 0;                  //clear timer
  TIFR2 |= (1<<OCF2A);        // clear interrupt flag in case it was already set
  TIMSK2 |= (1<<OCIE2A);       // enable interupt
  TCCR2B |= (1<<CS21); //CLK/8 //start timer with precaler of 8
  //Serial.println(TCCR3B,BIN);
}

void writer_init() {
   bit_TX = digitalPinToBitMask(output_pin);
  port_TX = digitalPinToPort(output_pin);
  if (port_TX == NOT_A_PIN)
 { Serial.println("BAD output PIN");
 return;
 }
direction_TX = portModeRegister(port_TX);
 out_TX = portOutputRegister(port_TX);
  // outputPIN output
  *direction_TX |= bit_TX; // pin  to output

  // Timer3 setup CLK/8, CTC on OCR0A match
  TCCR2A = (1<<WGM21);//
TCCR2B = 0; //CTC CLear timer on compare match enabled 
  // Timer0 Output Compare Interrupt A enable
  TIMSK2 = (1<<OCIE2A);  
  ASSR=0;
}

//void request_ID(uint8_t *id) {
//
//  unsigned char msg[5];
//  msg[0] = 0x68;
//  msg[1] = 0x6a;
//  msg[2] = 0xf0;
//  msg[3] = id[0];
//  msg[4] = id[1];
//
//  writer_send(msg, 5,0);
//}
//void request_ID(uint8_t id) {
//
//  unsigned char msg[5];
//  msg[0] = 0x68;
//  msg[1] = 0x6a;
//  msg[2] = 0xf0;
//  msg[3] = 0x01;
//  msg[4] = id;
//
//  writer_send(msg, 5,0);
//}

