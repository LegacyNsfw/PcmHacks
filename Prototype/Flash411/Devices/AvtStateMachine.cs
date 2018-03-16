using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    class AvtMessage
    {
        public DateTime Timestamp { get; private set; }
        public byte[] Data { get; private set; }

        public AvtMessage(DateTime timestamp, byte[] data)
        {
            this.Timestamp = timestamp;
            this.Data = data;
        }
    }

    class AvtStateMachine
    {
        private DateTime timestamp;
        private List<byte> currentMessage;
        private int state;
        private int bytesRemaining;

        public AvtStateMachine()
        {
            this.timestamp = DateTime.Now;
            this.currentMessage = new List<byte>();
            this.state = 0;
            this.bytesRemaining = 0;
        }

        public AvtMessage Push(byte value)
        {
            switch(state)
            {
                case 0: // this is the first byte received
                    this.bytesRemaining = value & 0x0F;
                    currentMessage.Add(value);
                    this.state = 1;
                    break;

                case 1:
                    currentMessage.Add(value);
                    this.bytesRemaining--;

                    if (bytesRemaining == 0)
                    {
                        // That was the last byte, so return the message.
                        return new AvtMessage(this.timestamp, this.currentMessage.ToArray());
                    }
                    this.state = 2;
                    break;

                case 2:
                    // This shouldn't happen.
                    break;
            }

            // Return null to indicate that we don't have a full message yet.
            return null;
        }
    }
}
