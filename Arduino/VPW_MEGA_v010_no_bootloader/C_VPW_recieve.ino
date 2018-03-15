void reader_init() {
  // For reader:
  // PC5 input, pull-up
  bit_RX = digitalPinToBitMask(input_pin);
  port_RX = digitalPinToPort(input_pin);
  if (port_RX == NOT_A_PIN)
 { Serial.println("BAD input PIN");
 return;
 }
 Serial.println("Hi");
direction_RX = portModeRegister(port_RX);
 out_RX = portOutputRegister(port_RX);
  in_RX = portInputRegister(port_RX);
  *out_RX |= bit_RX;  
  // enable pin change interrupt PCINT#
  if(digitalPinToPCICR(input_pin)==0)
  {
    Serial.println("not interupt pin");
    return;
  }
  *digitalPinToPCICR(input_pin) |= (1<<digitalPinToPCICRbit(input_pin));
  *digitalPinToPCMSK(input_pin) |= (1<<digitalPinToPCMSKbit(input_pin));

  
  reader_pin_last = *in_RX & bit_RX; // read pin state


  // Timer1 setup CLK/64
  OCR1A = 0x61A7; // will campare value of OCR1A against timer 1 for timeout
  TCCR1B = (1<<CS11) | (1<<CS10); //CLK/64
  TCCR1A =0;
    TCCR1C =0;
  // Timer1 enable Output Compare 1 match interrupt
  TIMSK1 |= (1<<OCIE1A); // if OCR1A matches timer interupt will happen
  // notes:
  //	100,000us = 0x61A7 (CTC timer max)
  //	     64us = 15 (small bit period)
  //	    128us = 29 (large bit period)
  //	    200us = 46 (Start Of Frame period)
  //	    768us = 177 (Break period)
}

uint16_t read_ID(uint8_t id){
  uint16_t veh_RPM;

  uint16_t loopy = 0;
  for(loopy=0; loopy<1000; loopy++) {
    delayMicroseconds(100);
    if(packet_waiting) {
      // see if it's the RPM return packet
      if(packet_data[0]==0x48 && packet_data[1]==0x6b && packet_data[2]==0x10 && packet_data[3]==0x41 && packet_data[4]==id) {
        // got RPM data
        veh_RPM = packet_data[5];
        veh_RPM = veh_RPM << 8;
        veh_RPM = veh_RPM + packet_data[6];
        //          delay_ms(30);
        //printf_P(PSTR("CODE %d = %d\r\n"),id,veh_RPM);
        return veh_RPM; 
      }
    }
  }
  return READ_TIMEOUT;
}
// This checksum code originally from Bruce Lightner,
// Circuit Cellar Issue 183, October 2005
// ftp://ftp.circuitcellar.com/pub/Circuit_Cellar/2005/183/Lightner-183.zip
unsigned char crc8buf(const unsigned char *buf, uint8_t len) {
  unsigned char val;
  unsigned char i;
  unsigned char chksum;

  chksum = 0xff;  // start with all one's
  while (len--) {
    i = 8;
    val = *buf++;
    while (i--) {
      if (((val ^ chksum) & 0x80) != 0) {
        chksum ^= 0x0e;
        chksum = (chksum << 1) | 1;
      } 
      else {
        chksum = chksum << 1;
      }
      val = val << 1;
    }
  }

  return ~chksum;
}

