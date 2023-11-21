using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PcmHacking
{
    public class CanMessage
    {
        public UInt32 MessageId { get; private set; }
        public ImmutableArray<byte> Payload { get; private set; }

        public bool IsRemoteFrame { get; private set; }

        public CanMessage(uint messageId, ImmutableArray<byte> payload, bool isRemoteFrame)
        {
            MessageId = messageId;
            Payload = payload;
            IsRemoteFrame = isRemoteFrame;
        }
    }

    public class CanParser
    {
        enum State
        {
            Start,
            ExpectAA = Start,
            ExpectType,
            ExpectExtendedIdByte1,
            ExpectExtendedIdByte2,
            ExpectExtendedIdByte3,
            ExpectExtendedIdByte4,
            ExpectStandardIdByte1,
            ExpectStandardIdByte2,
            ExpectMessageByte1,
            ExpectMessageByte2,
            ExpectMessageByte3,
            ExpectMessageByte4,
            ExpectMessageByte5,
            ExpectMessageByte6,
            ExpectMessageByte7,
            ExpectMessageByte8,
            Expect55,
            Complete = Start,
        }

        State state = State.Start;

        UInt32 messageId;

        int payloadLength;

        byte[] payload = new byte[8];

        bool isRemoteFrame;

        public CanParser()
        {

        }

        public bool IsCompleteMessage(byte b, out CanMessage message)
        {
            this.StateMachine(b);

            if (this.state == State.Complete)
            {
                message = new CanMessage(
                    this.messageId,
                    ImmutableArray.Create(this.payload, 0, this.payloadLength),
                    this.isRemoteFrame);

                return true;
            }

            message = null;
            return false;
        }

        public void StateMachine(byte value)
        {
            switch (state)
            {
                case State.ExpectAA:
                    if (value == 0xAA)
                    {
                        this.state = State.ExpectType;
                        this.messageId = 0;
                        this.payloadLength = 0;
                        this.isRemoteFrame = false;
                    }
                    break;

                case State.ExpectType:
                    this.isRemoteFrame = (value & 0x10) > 0;
                    this.payloadLength = (value & 0xF);
                    for (int index = 0; index < this.payload.Length; index++)
                    {
                        this.payload[index] = 0;
                    }

                    if ((value & 0x20) > 0)
                    {
                        this.state = State.ExpectExtendedIdByte1;
                    }
                    else
                    {
                        this.state = State.ExpectStandardIdByte1;
                    }
                    break;

                case State.ExpectStandardIdByte1:
                    this.messageId = value;
                    this.state = State.ExpectStandardIdByte2;
                    break;

                case State.ExpectStandardIdByte2:
                    this.messageId |= (UInt16) ((value & 7) << 8);
                    this.state = State.ExpectMessageByte1;
                    break;

                case State.ExpectExtendedIdByte1:
                    this.messageId = value;
                    this.state = State.ExpectExtendedIdByte2;
                    break;

                case State.ExpectExtendedIdByte2:
                    this.messageId |= ((UInt32)value << 8);
                    this.state = State.ExpectExtendedIdByte3;
                    break;

                case State.ExpectExtendedIdByte3:
                    this.messageId |= ((UInt32)value << 16);
                    this.state = State.ExpectExtendedIdByte4;
                    break;

                case State.ExpectExtendedIdByte4:
                    this.messageId |= ((UInt32)value << 24);
                    this.state = State.ExpectMessageByte1;
                    break;

                case State.ExpectMessageByte1:
                    this.payload[0] = value;
                    if (this.payloadLength == 1)
                    {
                        this.state = State.Expect55;
                    }
                    this.state = State.ExpectMessageByte2;
                    break;

                case State.ExpectMessageByte2:
                    this.payload[1] = value;
                    if (this.payloadLength == 2)
                    {
                        this.state = State.Expect55;                    
                    }
                    this.state = State.ExpectMessageByte3;
                    break;

                case State.ExpectMessageByte3:
                    this.payload[2] = value;
                    if (this.payloadLength == 3)
                    {
                        this.state = State.Expect55;
                    }
                    this.state = State.ExpectMessageByte4;
                    break;

                case State.ExpectMessageByte4:
                    this.payload[3] = value;
                    if (this.payloadLength == 4)
                    {
                        this.state = State.Expect55;
                    }
                    this.state = State.ExpectMessageByte5;
                    break;

                case State.ExpectMessageByte5:
                    this.payload[4] = value;
                    if (this.payloadLength == 5)
                    {
                        this.state = State.Expect55;
                    }
                    this.state = State.ExpectMessageByte6;
                    break;

                case State.ExpectMessageByte6:
                    this.payload[5] = value;
                    if (this.payloadLength == 6)
                    {
                        this.state = State.Expect55;
                    }
                    this.state = State.ExpectMessageByte7;
                    break;

                case State.ExpectMessageByte7:
                    this.payload[6] = value;
                    if (this.payloadLength == 7)
                    {
                        this.state = State.Expect55;
                    }
                    this.state = State.ExpectMessageByte8;
                    break;

                case State.ExpectMessageByte8:
                    this.payload[7] = value;
                    if (this.payloadLength == 8)
                    {
                        this.state = State.Expect55;
                    }
                    this.state = State.Expect55;
                    break;


                case State.Expect55:
                    if (value == 0x55)
                    {
                        state = State.Complete;
                    }
                    else
                    {
                        state = State.Start;
                    }
                    break;

                default:
                    state = State.Start;
                    break;
            }
        }
    }
}
