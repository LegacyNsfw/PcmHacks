
#define VERSION "VPW_MEGA_V010"
/*
 *
Check sum done by Transciever.
V010 Adjusted VPW timing slightly.  Removed print for "Not programmed"
V009a a "-not programmed" shows up during the read some times.  Not sure why.  Turned on print for that
VPW_MEGA V009 user configurable (via A_Dec_prog) length and packet size. CRC on and off, Supress non bin data during a read.
VPW_MEGA V007a turned on check bit.  Fixed ending address.
VPW_MEGA V007 Moved Akn to ECU response to tester present instead of message rec'd.
VPW_MEGA_V006 cleaned up not needed variables
VPW_MEGA_V005 removed prints for easier .bin capture
VPW_MEGA_V004 working!!!




 

 
 A_Data         :Boot loader is stored in this page
 A_Dec_prog     :Global Declarations that are needed for the OBDII subroutines
 A_MCP_DEC      :Declarations needed for the MCP2515 subroutines
 A_VPW_Dec      :Declarations that go with the VPW data and decode routines.
 B_main         :has void setup,loop,etc
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 */



