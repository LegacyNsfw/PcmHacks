const byte USE_POINTER =(1<<0);
const byte NO_CRC = (1<<1);

#define input_pin 12 ///must change which pin cange interupt is being use if
byte bit_RX;
byte port_RX;
volatile uint8_t *out_RX;
volatile uint8_t *in_RX;
volatile uint8_t *direction_RX;
long timer;


int w;
#define output_pin 5
byte bit_TX;
byte port_TX;
volatile uint8_t *out_TX;
volatile uint8_t *direction_TX;
#include <stdio.h>

#include <avr/io.h>
#include <avr/interrupt.h>
#include <avr/pgmspace.h>
#include <inttypes.h>

//#include "../libnerdkits/delay.h"
//#include "../libnerdkits/uart.h"
//#include "../libnerdkits/lcd.h"
//
boolean state;
byte test[]={0xAA,0xAA,0xAA,0xAA,0xAA,0xAA};
boolean resend_flag;
#define T_64US 0x7F
#define T_128US 0xFF
#define T_200US 0x18F
#define T_200US_2 (T_200US - 256)

volatile uint8_t writer_started = 0;
volatile uint8_t writer_extra = 0;
volatile uint16_t writer_size = 0;
unsigned char writer_buf[15];

unsigned char* writer_data = NULL;
volatile uint16_t writer_counter;
volatile uint8_t writer_bit_counter;
volatile uint8_t writer_collision;
unsigned char writerCRC;
bool calcCRC;
bool writeProgMem;
//const zeroForWrite=0;

volatile uint16_t reader_size = 0;
volatile unsigned char reader_data[4200];// size of buffer to store data read until we send it to the serial monitor
volatile uint8_t reader_pin_last=0;
volatile char reader_byte=0;
volatile uint8_t reader_byte_counter=0;

volatile uint8_t reader_started = 0;
volatile unsigned char packet_data[15];
volatile uint8_t packet_waiting;
volatile uint16_t packet_size;






int r=0;
boolean start=1;
#define READ_TIMEOUT 65535





