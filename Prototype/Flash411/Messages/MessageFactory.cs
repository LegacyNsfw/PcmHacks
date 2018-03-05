using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    class MessageFactory
    {
        public Message CreateReadRequest(byte block)
        {
            byte[] bytes = new byte[] { 0x6C, 0x10, 0xF0, 0x3C, block };
            return new Message(bytes);
        }

        public Message CreateOperatingSystemIdReadRequest()
        {
            return CreateReadRequest(BlockId.OperatingSystemId);
        }

        public Message CreateVinRequest1()
        {
            return CreateReadRequest(BlockId.Vin1);
        }

        public Message CreateVinRequest2()
        {
            return CreateReadRequest(BlockId.Vin2);
        }

        public Message CreateVinRequest3()
        {
            return CreateReadRequest(BlockId.Vin3);
        }

        public Message CreateSeedRequest()
        {
            byte[] bytes = new byte[] { 0x6C, 0x10, 0xF0, 0x27, 0x01 };
            return new Message(bytes);
        }

        public Message CreateUnlockRequest(UInt16 key)
        {
            byte keyHigh = (byte)((key & 0xFF00) >> 8);
            byte keyLow = (byte)(key & 0xFF);
            byte[] bytes = new byte[] { 0x6C, 0x10, 0xF0, 0x27, 0x02, keyHigh, keyLow };
            return new Message(bytes);
        }
    }
}
