using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PcmHacking
{
    public class CanLogger
    {
        public class ParameterValue
        {
            public string Name { get; set; }
            public string Units { get; set; }
            public string Value { get; set; }
        }

        private IPort canPort;

        public CanLogger()
        {
        }

        public async Task SetPort(IPort port)
        {
            this.canPort = port;
            SerialPortConfiguration configuration = new SerialPortConfiguration();
            configuration.BaudRate = 115200;
            configuration.DataReceived = this.DataReceived;
            await this.canPort.OpenAsync(configuration);

            Thread.Sleep(250);
            this.keySnapshot.Clear();

            foreach (UInt32 key in this.messages.Keys)
            {
                this.keySnapshot.Add(key);
            }

            this.keySnapshot.Sort();
        }

        Dictionary<UInt32, ParameterValue> messages = new Dictionary<uint, ParameterValue>();

        List<UInt32> keySnapshot = new List<UInt32>();

        public void DataReceived(byte[] buffer, int bytesReceived)
        {
            for(int i = 0; i < bytesReceived; i++)
            {
                this.StateMachine(buffer[i]);
                if (this.state == State.Done)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int payloadIndex = 0; payloadIndex < 8; payloadIndex++)
                    {
                        builder.Append(messageData[payloadIndex].ToString("X2"));
                    }

                    ParameterValue pv = this.TranslateValue();
                    messages[this.messageId] = pv;
                }
            }
        }

        /// <summary>
        /// Is this needed?
        /// </summary>
        public void Start()
        {

        }

        public void Stop()
        {

        }

        private ParameterValue TranslateValue()
        {
            ParameterValue result = new ParameterValue();
            int valueRaw = 0;
            double value;
            switch (this.messageId)
            {
                case (uint)0x000a0302:
                    valueRaw = (this.messageData[0] << 8) | this.messageData[1];
                    value = valueRaw;
                    value = (value * 1.8) + 32.0;
                    result.Value = ((int)value).ToString();
                    result.Units = "F";
                    result.Name = "Temperature";
                    return result;

                case (uint)0x00000180:
                    valueRaw = (this.messageData[0] << 8) | this.messageData[1];
                    value = valueRaw;
                    value = (value * 0.0001) * 14.7;
                    result.Value = value.ToString();
                    result.Units = "AFR";
                    result.Name = "AFR 1";
                    return result;

                case (uint)0x00000181:
                    valueRaw = (this.messageData[0] << 8) | this.messageData[1];
                    value = valueRaw;
                    value = (value * 0.0001) * 14.7;
                    result.Value = value.ToString();
                    result.Units = "AFR";
                    result.Name = "AFR 2";
                    return result;

                default:
                    valueRaw = (this.messageData[0] << 8) | this.messageData[1];
                    result.Value = valueRaw.ToString();
                    result.Units = "raw";
                    result.Name = this.messageId.ToString("X8");
                    return result;
            }
        }

        public IEnumerable<string> GetParameterNames()
        {
            foreach (UInt32 key in this.keySnapshot)
            {
                yield return this.messages[key].Name + "(" + this.messages[key].Units + ")";
            }
        }

        public IEnumerable<ParameterValue> GetParameterValues()
        {
            if (this.keySnapshot.Count == 0)
            {
                this.GetParameterNames();
            }

            foreach(UInt32 key in this.keySnapshot)
            {
                yield return this.messages[key];
            }
        }

        UInt32 messageId = 0;
        byte[] messageData = new byte[8];

        enum State
        {
            Start,
            SeenFF,
            SeenIdByte1,
            SeenIdByte2,
            SeenIdByte3,
            SeenIdByte4,
            SeenFE,
            SeenMessageByte0,
            SeenMessageByte1,
            SeenMessageByte2,
            SeenMessageByte3,
            SeenMessageByte4,
            SeenMessageByte5,
            SeenMessageByte6,
            Done,
        }

        State state = State.Start;

        public void StateMachine(byte value)
        {
            switch (state)
            {
                case State.Start:
                    if (value == 0xFF)
                    {
                        state = State.SeenFF;
                        messageId = 0;
                    }
                    break;

                case State.SeenFF:
                    messageId |= value;
                    state = State.SeenIdByte1;
                    break;

                case State.SeenIdByte1:
                    messageId <<= 8;
                    messageId |= value;
                    state = State.SeenIdByte2;
                    break;

                case State.SeenIdByte2:
                    messageId <<= 8;
                    messageId |= value;
                    state = State.SeenIdByte3;
                    break;

                case State.SeenIdByte3:
                    messageId <<= 8;
                    messageId |= value;
                    state = State.SeenIdByte4;
                    break;

                case State.SeenIdByte4:
                    if (value == 0xFE)
                    {
                        state = State.SeenFE;
                        for (int index = 0; index < messageData.Length; index++)
                        {
                            messageData[index] = 0;
                        }
                    }
                    else
                    {
                        state = State.Start;
                    }
                    break;

                case State.SeenFE:
                    messageData[0] = value;
                    state = State.SeenMessageByte0;
                    break;

                case State.SeenMessageByte0:
                    messageData[1] = value;
                    state = State.SeenMessageByte1;
                    break;

                case State.SeenMessageByte1:
                    messageData[2] = value;
                    state = State.SeenMessageByte2;
                    break;

                case State.SeenMessageByte2:
                    messageData[3] = value;
                    state = State.SeenMessageByte3;
                    break;

                case State.SeenMessageByte3:
                    messageData[4] = value;
                    state = State.SeenMessageByte4;
                    break;

                case State.SeenMessageByte4:
                    messageData[5] = value;
                    state = State.SeenMessageByte5;
                    break;

                case State.SeenMessageByte5:
                    messageData[6] = value;
                    state = State.SeenMessageByte6;
                    break;

                case State.SeenMessageByte6:
                    messageData[7] = value;
                    state = State.Done;
                    break;

                default:
                    state = State.Start;
                    break;
            }
        }
    }
}
