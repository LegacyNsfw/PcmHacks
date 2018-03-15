
ISR(TIMER2_COMPA_vect) { //compare match interupt

  //    Serial.println(TCNT3,HEX);


  if (!writer_started) //if not writing leave
    return;
  // to extend period more than 256
  if (writer_extra > 0) {
    OCR2A = writer_extra;
    writer_extra = 0;
    return;
  }

  // check for collision
  if (writer_collision) {
    // digitalWrite(LED_BUILTIN,HIGH);
    // stop writing!  de-assert.
    *out_TX &= ~bit_TX;	// should be de-asserted anyway.
    writer_started = 0; // stop writing
    TIMSK2 &= ~(1 << OCIE2A); //stop interupt
    TCCR2B &= ~(1 << CS21); //stop timer
    //Serial.println("collision");//debug
    resend_flag = 1;
    writer_collision = 0;
    return;
  }
   if ((!calcCRC&&writer_counter == writer_size)||(calcCRC&&writer_counter == writer_size+1)) { //finnished with message // override in case of end-of-frame
     *out_TX  &= ~bit_TX; //low pin
    OCR2A = 0xFF;                  //set time for end of frame
    writer_extra = T_200US_2;
   }
  else
  {
  uint8_t this_bit;
  if(writeProgMem) this_bit = pgm_read_byte_near(writer_data+writer_counter) >> (7 - writer_bit_counter); //shift over to bit
  else  this_bit = writer_data[writer_counter] >> (7 - writer_bit_counter); //shift over to bit
  this_bit = this_bit & 0x01;    //issolate bit
  //Serial.println(writer_bit_counter,HEX);
  // toggle bit and set timer
  if ((writer_bit_counter & 0x01) == 0) { //if pin will be low now
    // bits 0,2,4,6
    *out_TX  &= ~bit_TX; //low pin
    if (this_bit) { // 1 or 0;
      OCR2A = T_128US; //set time long period
    }
    else {
      OCR2A = T_64US;  //set time short period
    }
  }
  else { //pin is going to be high
    // bits 1,3,5,7
    *out_TX |= bit_TX; // high pin
    if (this_bit) { //1 or 0
      OCR2A = T_64US; //short period
    }
    else {
      OCR2A = T_128US; //long period
    }
  }
  }
  // update counters
  writer_bit_counter++;
  if (writer_bit_counter == 8) {
    writer_counter++;
    writer_bit_counter = 0;
    if(calcCRC&&writer_counter == writer_size)
   {
     writer_data = &writerCRC-writer_counter;// slightly nasty but will make next call to writer data be the crc(breaks reference to the rest of message however
     writeProgMem = false; //crc not in prog mem
   }
  }
  // when done, disable our own interrupt from firing.
  if (((!calcCRC&&writer_counter == writer_size)||(calcCRC&&writer_counter == writer_size+1)) && (writer_bit_counter == 1)) {
    TIMSK2 &= ~(1 << OCIE2A); // disable interupt
    TCCR2B &= ~(1 << CS21); //stop timer
    *out_TX &= ~bit_TX; //low pin should be low any ways
    writer_started = 0; //stop writing
  }
}
//###############END SEND VOID#############################3


//########################## Read #######################
ISR(PCINT0_vect) { //if pin has changed
  // record timer value
  uint16_t timer_val = TCNT1; //get timer value
  TCNT1 = 0; //clear timer
  uint8_t cur_pin_value = *in_RX & bit_RX; //get pin value
  if (cur_pin_value > 0)cur_pin_value = 1;

  cur_pin_value = !cur_pin_value;
  //return;


  // do collision checking magic if writer is active
  if (writer_started) { //if we are writing
    // check for collision
    if ((*out_TX & bit_TX) == 0 && cur_pin_value == 0) { // if we are writing low and the bus is high
      // we're not dominant.

      writer_collision = 1;

    }// end if if writing low and pin is high
    else
    {
      if (writer_size > 15)return; // dont read our own super long messages
      // return;//leave
    }
  }// end if writer is started


  //verify that a transition has happened
  if (cur_pin_value == reader_pin_last || timer_val < 8)
  {
    return;	// glitched transition; ignore
  }

  reader_pin_last = cur_pin_value;

  // discriminate based on timer_val
  uint8_t bit_choice;
  if (timer_val < 0x2E) {	//if length of a bit 0x27= 160.6us Gchanged //was 27
    if (!reader_started)
      return;

    if (timer_val < 0x1C) {		// if short period 0x17=95.4us Gchanged //was 17
      // small period (64us)
      bit_choice = cur_pin_value ? 1 : 0; //if pin high 1 if low 0
    }
    else { //else it is large period
      // large period (128us)
      bit_choice = cur_pin_value ? 0 : 1; //if pin low 1 if pin high 0
    }

    // YAY! we can have new bit
    reader_byte = (reader_byte << 1); //shift temp byte over
    reader_byte |= bit_choice; //add new bit
    reader_byte_counter++; //count bits
    if (reader_byte_counter == 8) { // if full byte
      // DOUBLE YAY! we can has new byte
      //Serial.println("reader_byte");
      
        reader_data[reader_size] = reader_byte; //put byte in array
        reader_byte_counter = 0; //clear  bit counter
        reader_byte = 0;  //clear temp byte
        reader_size++;    //count bytes
        
    }
    return;
  } //end if 160 us

  else if (timer_val < 0x41) {	// if length of start of frame 0x41=264.8us Gchanged
    // Start Of Frame (200us)
    if (cur_pin_value == 1 && !reader_started) { // if pin high  and reader has not started
      reader_started = 1; //start reader
      reader_byte_counter = 0; // clear bit counter
      reader_byte = 0; //clear temp byte
      reader_size = 0; //clear byte counter
      // change timer overflow to 200us
      OCR1A = 0x31; // set compare match to look for EOF 0x31=200us Gchanged

    }
    return;

  }

  return;
}// ############## end of read void


//############ end of frame or time out  #################
ISR(TIMER1_COMPA_vect) { //reader compare match

  if (reader_started) { // if reader has started
    // indicates End Of Frame OR reader timeout
    if (OCR1A == 0x61A7) { //if time outg changed
      // was timeout
      reader_started = 0; //stop reading
    }
    else { //if EOF
      // was end of frame
      // change timer overflow to 100ms
      OCR1A = 0x61A7; //compare match to look for overflow 0x61A7=100ms g changed
      reader_started = 0; //stop reading
   
      unsigned char cksum = crc8buf((unsigned char *)reader_data, (reader_size - 1)); //calculate checksum
      if (cksum != reader_data[reader_size - 1]) { //if error
      }
      // copy all  bytes to the packet buffer
      uint8_t i;
      for (i = 0; i < 15; i++) {
        packet_data[i] = reader_data[i]; //copy bytes
      }
      packet_size = reader_size;
      packet_waiting = (reader_size > 0) ? 1 : 0; //if length greater than 0 packet is waiting
      //
      // stop the reader
    }//end else (is EOF)



  } //end reader started
  else {  }
}//end void


ISR(BADISR_vect)
{
  Serial.println("ERROR");
    // user code here
}

