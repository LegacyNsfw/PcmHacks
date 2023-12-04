using PcmHacking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PcmHacking
{
    public class CanLogger : IDisposable
    {
        public class ParameterValue
        {
            public string Name { get; set; }
            public string Units { get; set; }
            public string Value { get; set; }

            public override string ToString()
            {
                return this.Name;
            }
        }

        private IPort canPort;
        private CanParser parser = new CanParser();
        List<UInt32> keySnapshot = new List<UInt32>();

        // Note that this is accessed by multiple threads, so it must only be used within "lock(messages)"
        Dictionary<UInt32, ParameterValue> messages = new Dictionary<uint, ParameterValue>();

        public CanLogger()
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.canPort?.Dispose();
            }
        }

        public async Task SetPort(IPort port)
        {
            this.canPort?.Dispose();
            this.canPort = port;

            // Remove all known messages
            this.keySnapshot.Clear();

            lock (this.messages)
            {
                this.messages.Clear();
            }

            if (this.canPort != null)
            {
                SerialPortConfiguration configuration = new SerialPortConfiguration();
                configuration.BaudRate = 2000000;
                configuration.DataReceived = this.DataReceived;
                await this.canPort.OpenAsync(configuration);

                // Discover what messages are available on the bus.
                Thread.Sleep(1500);
                lock (this.messages)
                {
                    foreach (UInt32 key in this.messages.Keys)
                    {
                        this.keySnapshot.Add(key);
                    }
                }

                this.keySnapshot.Sort();
            }
        }

        public void DataReceived(byte[] buffer, int bytesReceived)
        {
            for(int i = 0; i < bytesReceived; i++)
            {
                CanMessage message;
                if (this.parser.IsCompleteMessage(buffer[i], out message))
                {
                    ParameterValue pv = this.TranslateValue(message);
                    if (pv != null)
                    {
                        lock (this.messages)
                        {
                            this.messages[message.MessageId] = pv;
                        }
                    }
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

        private ParameterValue TranslateValue(CanMessage message)
        {
            ParameterValue result = new ParameterValue();
            int valueRaw = 0;
            double value;
            switch (message.MessageId)
            {
                case (uint)0x000a0301:
                    valueRaw = (this.messageData[0] << 8) | this.messageData[1];
                    value = valueRaw;
                    value = value * 0.01; // bar
                    value = value * 14.5037738; // psi
                    result.Value = ((int)value).ToString("0.00");
                    result.Units = "F";
                    result.Name = "AEM Pressue";
                    return result;

                case (uint)0x000a0302:
                    valueRaw = (message.Payload[0] << 8) | message.Payload[1];
                    value = valueRaw;
                    value = (value * 1.8) + 32.0;
                    result.Value = ((int)value).ToString("0.00");
                    result.Units = "F";
                    result.Name = "AEM Temperature";
                    return result;

                case (uint)0x00000180:
                    valueRaw = (message.Payload[0] << 8) | message.Payload[1];
                    value = valueRaw;
                    value = (value * 0.0001) * 14.7;
                    result.Value = value.ToString("0.00");
                    result.Units = "AFR";
                    result.Name = "AEM AFR 1";
                    return result;

                case (uint)0x00000181:
                    valueRaw = (message.Payload[0] << 8) | message.Payload[1];
                    value = valueRaw;
                    value = (value * 0.0001) * 14.7;
                    result.Value = value.ToString("0.00");
                    result.Units = "AFR";
                    result.Name = "AEM AFR 2";
                    return result;

                default:
                    valueRaw = (message.Payload[0] << 8) | message.Payload[1];
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
                string name;
                lock(this.messages)
                {
                    name = this.messages[key].Name + "(" + this.messages[key].Units + ")";
                }
                yield return name;
            }
        }

        public IEnumerable<ParameterValue> GetParameterValues()
        {
            foreach(UInt32 key in this.keySnapshot)
            {
                ParameterValue value;
                lock(this.messages)
                {
                    value = this.messages[key];
                }
                yield return value;
            }
        }

        UInt32 messageId = 0;
        byte[] messageData = new byte[8];

    }
}
