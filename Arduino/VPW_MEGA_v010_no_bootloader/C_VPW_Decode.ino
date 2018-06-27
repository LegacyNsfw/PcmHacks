void VPW_Decode()
{
  int i2;

  if(VPW_len==0) return; //if there is a nulll message do nothing and leave.


  if(!Supress_Non_BIN||Have_Key < 9) //don't print when we are reading a .bin (key 8) with supress on.
  {
    Print_MSG_Nln();
    //from
/*
    Serial.print("- ");

    for (int i=0; i<2; i++){
      if(i==0) i2=2;
      else i2=1;

      if(VPW_data[i2]==0x8||VPW_data[i2]==0x8+1) Serial.print("Engine Torque"); 
      else if(VPW_data[i2]==0xA||VPW_data[i2]==0xA+1) Serial.print("Air Intake"); 
      else if(VPW_data[i2]==0xC||VPW_data[i2]==0xC+1) Serial.print("Engine"); 
      else if(VPW_data[i2]==0x10||VPW_data[i2]==0x10+1) Serial.print("ECU"); 
      else if(VPW_data[i2]==0x12||VPW_data[i2]==0x12+1) Serial.print("Throttle"); 
      else if(VPW_data[i2]==0x14||VPW_data[i2]==0x14+1) Serial.print("A/C Clutch"); 
      else if(VPW_data[i2]==0x1A||VPW_data[i2]==0x1A+1) Serial.print("Engine RPM"); 
      else if(VPW_data[i2]==0x20||VPW_data[i2]==0x20+1) Serial.print("Chassis"); 
      else if(VPW_data[i2]==0x24||VPW_data[i2]==0x24+1) Serial.print("Wheels"); 
      else if(VPW_data[i2]==0x28||VPW_data[i2]==0x28+1) Serial.print("Vehicle Speed"); 
      else if(VPW_data[i2]==0x3A||VPW_data[i2]==0x3A+1) Serial.print("Trans"); 
      else if(VPW_data[i2]==0x48||VPW_data[i2]==0x48+1) Serial.print("Coolant Temp"); 
      else if(VPW_data[i2]==0x4A||VPW_data[i2]==0x4A+1) Serial.print("Engine Oil"); 
      else if(VPW_data[i2]==0x53||VPW_data[i2]==0x53+1) Serial.print("Engine Systems");
      else if(VPW_data[i2]==0x58||VPW_data[i2]==0x58+1) Serial.print("Suspension");
      else if(VPW_data[i2]==0x74||VPW_data[i2]==0x74+1) Serial.print("Electrical Energy MGT");
      else if(VPW_data[i2]==0x82||VPW_data[i2]==0x82+1) Serial.print("Fuel System");
      else if(VPW_data[i2]==0x86||VPW_data[i2]==0x86+1) Serial.print("Ignition");
      else if(VPW_data[i2]==0x92||VPW_data[i2]==0x92+1) Serial.print("Security");
      else if(VPW_data[i2]==0xEA||VPW_data[i2]==0xEA+1) Serial.print("Displays"); 
      else if(VPW_data[i2]==0xF0||VPW_data[i2]==0xF0+1) Serial.print("Tester"); 
      else if(VPW_data[i2]==0xF2||VPW_data[i2]==0xF2+1) Serial.print("Exterior Env."); 
      else if(VPW_data[i2]==0xF4||VPW_data[i2]==0xF4+1) Serial.print("Interior Env."); 
      else if(VPW_data[i2]==0xFA||VPW_data[i2]==0xFA+1) Serial.print("VIN"); 
      else if(VPW_data[i2]==0xFE||VPW_data[i2]==0xFF) Serial.print("All"); 

      if(i==0)Serial.print(" to ");

    }
    Serial.print(" ");//space for a bank
    //to
    */
  }

  switch(VPW_data[3])//Mode byte
  {               

   
    //*********************************************************************
  case 0x27:  			//  Mode Security Request

    if(VPW_data[4]==0x01) Serial.print("- Requesting Seed **********************************************************"); //sub mode 01 (27 01)
    if(VPW_data[4]==0x02) //if sub mode 2 (27 02)
    {  
     /*
      Serial.print("- Attempting Key: ");
      Serial.print(VPW_data[5],HEX);
      Serial.print(" ");
      Serial.print(VPW_data[6],HEX);
      Serial.print(" **************************************************** "); 
      */
    }

    break;

  case 0x67:  			//  Mode 67 Security Response

    //--------------------------------------- mode 67 01 ---------------------------------------------------------

    if(VPW_data[4]==0x01&&VPW_data[5]!=0x37) {//response is ***...67 01***, (Resp. Security, seed) and not a time out (67 01 37).  Alter this to not use 37 but check for length (37 in byte 5 could be the seed first byte)
      Seed = word(VPW_data[5],VPW_data[6]);
      Have_Seed = 0;  //mark has the seed

      Serial.print("- Seed is: "); 
      //Serial.print(Seed,HEX);
      Serial.print(" **************************************************** "); 





/*


      //send key attempt
      Send_key[5] = highByte(Key);
      Send_key[6] = lowByte(Key);
      send_VPW(Send_key,sizeof(Send_key)); //send command

    } //end recieved Key

      if(VPW_data[4]==0x01&&VPW_data[5]==0x37){//Alter this to not just use 37 but check for length (37 in byte 5 could be the seed first byte)
      Serial.print(VPW_len);
      Serial.print(" ");
      Serial.print("(37)timeout not met "); 
      Have_Seed = 0; 
      Seed_Wait=10010;
      Have_Seed = 0;
*/
    
    }
    

    //--------------------------------------- mode 67 02 ---------------------------------------------------------


    if(Have_Seed == 1&&VPW_data[4]==0x02){//We have recieved the key message and response is ***...67 02***
      if(VPW_data[5]==0x34){
        Akn = 1; 
        Serial.print("- Key Accepted ********************************************************************************************");
      }//recived key Serial.print(" Valid Key, access granted";)Serial.print(Key,HEX);
      else{
        Have_Seed = 0;  //need to ask for seed again
        time_Pid2=millis(); //reset timer
        if(VPW_data[5]==0x35){
          Serial.print(Key,HEX);
          Serial.print(" (35)first try, Not Valid");  
          Seed_Wait=500;
        }//wait 100 ms for second try
        if(VPW_data[5]==0x36){
          Serial.print(Key,HEX);
          Serial.print(" (36)Second try, Not Valid");
          Have_Seed = 0; 
          Seed_Wait=10010;
          Serial.print(" ");
        }//wait ten seconds
        //   else if (Key != 0xFFFF)Key++; //if we haven't reach FFFF or it's not valid increment
        //   else if (Key != 0xFF)Key++; //if we haven't reach FFFF or it's not valid increment
        else {
          Serial.print(Key,HEX);
          Serial.print(" hit max value without access");
          Have_Key = 1;
        }
      }//end if it's not a valid key

      if(VPW_data[3]==0x7F){
        Serial.print("****General error (7F)***");
        Have_Seed = 0; 
        Seed_Wait=10010;
        Serial.print(" ");
      }//This is an error.  wait ten seconds

    } //end if key request response
    break;


    //******************************** Mode 28 *************************************
  case 0x28:  			//  Mode
    if(VPW_Message=="6CFEF02800")//Disable normal messages
    {
      Serial.print("- Disable normal messages (28)");
    }
    else Serial.print("- (28)");
    break;

    //******************************** Mode 68 *************************************
  case 0x68:  			//  Mode

    if(VPW_data[2]==0x10&&VPW_data[4]==0x00&&Have_Key > 1){//6C F0 10 68 00 correct response.  Set flag to ready to send
      Akn=1;
      Serial.print("- Disable normal messages ack (28 response)");
    }
    else Serial.print("- (68)");
    break;

  


    //******************************** Mode 34 -write *************************************
  case 0x34:  			//  Mode
    //6c f1 10 74 00 44 correct response to request to write


    break;

    //******************************** Mode 74 -write resposne*************************************
  case 0x74:  			//  Mode

    if(VPW_data[2]==0x10&&VPW_data[4]==0x00&&VPW_data[5]==0x44&&Have_Key > 1){//6c f1 10 74 00 44 correct response to request.  Set flag to ready to send
      Akn=1;
      Serial.print("  Read to Receive");
    }
    Serial.print("  write resp. (74): ");

    break;

    //******************************** Mode 35 -Read *************************************
  case 0x35:  			//  Mode
    if(!Supress_Non_BIN&&VPW_data[4]==0x1){ //don't print if supressing
      Serial.print("Read Request Num Bytes: ");
      word Number_Bytes = word(VPW_data[5],VPW_data[6]);
      Serial.print(Number_Bytes);
      Serial.print(" Address:");
      //if(VPW_data[7]=0)Serial.print("00");
      // Serial.print(VPW_data[7],HEX);
      //if(VPW_data[8]=0)Serial.print("00");
      for (int i=7; i<10; i++){
        Serial.print(" ");
        if(VPW_data[i]<=0x0F) Serial.print("0"); //if a single character add a leading zero.
        Serial.print(VPW_data[i],HEX);
      }


      //   if(VPW_data[7]<=0x0F) Serial.print("0"); //if a single character add a leading zero.
      //   Serial.print(VPW_data[7],HEX);
      //   Serial.print(" ");
      //   if(VPW_data[8]<=0x0F) Serial.print("0"); //if a single character add a leading zero.
      //  Serial.print(VPW_data[8],HEX);
      //    Serial.print(" ");
      //    if(VPW_data[9]<=0x0F) Serial.print("0"); //if a single character add a leading zero.
      // Serial.print(VPW_data[9],HEX);

    }
    break;

    //******************************** Mode 75 -Read resposne*************************************
  case 0x75:  			//  Mode

    //6C F0 10 75 01 52 bad response
    if(VPW_data[4]==0x1&&VPW_data[5]==0x52) Serial.print("  Read denied");
    //   else Serial.print("  read request resp. (75): ");


    break;

    //******************************** Mode 36 - Transfer *************************************
  case 0x36:  			//  Mode
    //6d f1 10 76 00 73 correct response to request to Read
    Serial.print("- Block transfer request (36): ");

    break;

    //******************************** Mode 76 -Transfer resposne*************************************
  case 0x76:  			//  Mode
    //6d f1 10 76 00 73 correct response to request to Read
    if(VPW_data[2]==0x10&&VPW_data[4]==0x00&&VPW_data[5]==0x73&&Have_Key > 1){//6c f1 10 74 00 44 correct response to request.  Set flag to ready to send
      Akn=1;
      Serial.print("  Data Received");
    }
    if(VPW_data[4]==0x0&&VPW_data[5]==0x77) Serial.print("  Timed out waiting for message");
    if(VPW_data[4]==0x0&&VPW_data[5]==0x78) Serial.print("  Busy, wait?");
    if(VPW_data[4]==0x0&&VPW_data[5]==0x78) Serial.print("  Transfer Complete");
    Serial.print("- Transfer request resp. (76): ");


    break;


    //******************************** Mode 3C *************************************
  case 0x3C:  			//  Mode: read data block
    Serial.print("");
    break;

    //******************************** Mode 7C (3C response)*************************************
  case 0x7C:  			//  Mode: read data block
    if(VPW_data[2]==0x10&&VPW_data[4]==0xBB&&VPW_data[5]==0x44&&VPW_data[6]==0x71){//6C F0 10 7C BB 44 71 correct response to request.  Set flag to ready to send
      Akn=1;
      Serial.print("3c response to read data blocks");
    }
    else      
      if(VPW_data[2]==0x10&&VPW_data[4]==0x21&&VPW_data[5]==0x00&&VPW_data[6]==0x07){//6C F0 10 7C 21 00 07 xx xx correct response to request.  Set flag to ready to send
      Akn=1;
      Serial.print("  3c response to read data blocks");
    }
    else Serial.print("     (7C)Responce");
    break;

    //******************************** Mode 3F *************************************
  case 0x3F:  			//  Mode: read data block

    //  if(VPW_Message=="6CFEF03F")//Tester present
    if(VPW_data[2]==0xF0||VPW_data[2]==0xF1)
    {
      if(!Supress_Non_BIN) Serial.println("- Tester Present");
      if(Have_Key==9) {
        Have_Key=8;
        Akn = 1;
      }
      if(Have_Key==10) Akn = 1;
    }
    if(!Supress_Non_BIN) Serial.print(" ");
    break;

    //******************************** Mode 7F (3F) resonse?*************************************
  case 0x7F:  			//  Mode: read data block

    Serial.print("- 7F (3F response)");

    break;

   
    //*********************************************************************
  case 0x60:  			//  Mode 

    if(VPW_Message=="ECF01060"||VPW_Message=="6CF01060"){
      Serial.print("- Return to Normal Mode");
      Read_Start = 1; 
    }
    
    break;


    //******************************Mode A0************************************
  case 0xA0:  			//  Mode 
    if(VPW_Message=="6C10F0A0") {
      Serial.print("- Normal Mode (A0) new: ");
    }

    break;

    //**************************Mode E0*********************************
  case 0xE0:  			//  Mode Reponse to A0
    if(VPW_Message=="6CF010E0AA") {
      Serial.print("- Normal Mode ack (E0) new: ");
    }

    break;



    //******************************** Mode AE *************************************
  case 0xAE:  			//  Mode Command
    if(VPW_data[4]==0x01)//Sub mode 1
    {
      if(VPW_Message=="AE01000000000000") Serial.print("Return command to ECU: " );
          }
          
    else 
    {      

      Serial.print("- Command Mode (AE) new: ");

    }

    break;



    //******************************** Mode EE (response to AE) *************************************
  case 0xEE:  			//  Mode Command
    if(VPW_Message=="6CF010EE01E1") Serial.print("Response to command " );

    else 
    {        

      Serial.print("- Command Mode (AE) new: ");

    }

    break;



    //********************************             Default                     *************************************
//  default:

 


  } //End Switch case

 if(!Supress_Non_BIN||Have_Key < 9) Serial.println(" ");
  return;
}//End VPW_Decode

/*
void VPW_Decode2()
 {
 //  Serial.print("got vpw message");
 word RPM_A;
 //if the data is the RPM message in the data stream calculate RPM.
 if(VPW_data[0] == 0x88&&VPW_data[1] == 0x1B&&VPW_data[2] == 0x10&&VPW_data[3] == 0x10){
 RPM =  word(VPW_data[4], VPW_data[5])/4; // convert GM RPM to real RPM
 //   Serial.print(String(RPM)+" "+String(word(VPW_data[4], VPW_data[5])/4));
 // RPM_A = engine_data * 6.4; // Convert to BMW hex RPM value
 //    Serial.print(engine_data);
 //  rMSB = highByte(RPM_A);
 //rLSB = lowByte(RPM_A);
 }
 
 //if the data is coolant temperature message in the data stream calculate Temp.
 if(VPW_data[0] == 0x68&&VPW_data[1] == 0x49&&VPW_data[2] == 0x10&&VPW_data[3] == 0x10){
 
 ECT =  VPW_data[4] - 40; //Calculate output value
 
 //  OILvalue2 = (engine_data + 48.373); //Since Car has no oil temp pickup then use Coolant temp.
 
 //Serial.print("***************************Engine temp ");
 //   Serial.print(engine_data);
 
 if(ECT>124){ // lights light if value is 229 (hot) set overheat light to on
 tempLight = 0x08;  // hex 08 = Overheat light on
 }
 else { //set overheat light to off
 tempLight = 0x00; // hex 00 = overheat light off
 }
 
 }
 
 if(VPW_data[0] == 0x8A&&VPW_data[1] == 0xEA&&VPW_data[2] == 0x10&&VPW_data[4] == 0xBD){
 
 if(bitRead(VPW_data[3],7))
 {
 //   Serial.print("************************************mil on");
 bitSet(ErrorState,1); //Set check engine Binary 0010 (hex 02). (if MIL is on bit 7 of VPW_data[2])
 }
 else{
 bitClear(ErrorState,1); //Set check engine Binary 0010 (hex 02). (if MIL is on bit 7 of VPW_data[2])
 //  Serial.print("************************************mil off");
 }
 }
 
 
 
 
 if(VPW_data[0] == 0x48&&VPW_data[1] == 0x6B&&VPW_data[2] == 0x10){
 
 //   Serial.print(" recieved PID");
 Decode(); 
 }
 // if(id[CAN2] == 0xC9)Decode_RPM(CAN2);
 
 
 }//end decode
 */











































