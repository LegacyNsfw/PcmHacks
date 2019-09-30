using DynamicExpresso;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    public class LogFileWriter
    {
        private StreamWriter writer;
        private DateTime startTime;

        public LogFileWriter (StreamWriter writer)
        {
            this.writer = writer;
        }

        public async Task WriteHeader(LogProfile profile)
        {
            this.startTime = DateTime.Now;
            string text = profile.GetParameterNames(", ");
            await this.writer.WriteAsync("Clock Time, Elapsed Time, ");
            await this.writer.WriteLineAsync(text);
        }

        public async Task WriteLine(string[] values)
        {
            await this.writer.WriteAsync(DateTime.Now.ToString("u"));
            await this.writer.WriteAsync(", ");
            await this.writer.WriteAsync(DateTime.Now.Subtract(this.startTime).ToString());
            await this.writer.WriteAsync(", ");
            await this.writer.WriteLineAsync(string.Join(", ", values));
        }
    }
    
    public class LogRowParser
    {
        private LogProfile profile;
        private Dictionary<byte, byte[]> responseData = new Dictionary<byte, byte[]>();
        private byte lastId;

        public bool IsComplete { get; private set; }

        public LogRowParser(LogProfile profile)
        {
            this.profile = profile;

            // Create a place to put response data, and get the ID of the last group.
            foreach(ParameterGroup group in this.profile.ParameterGroups)
            {
                this.lastId = (byte)(uint)group.Dpid;
                responseData[this.lastId] = new byte[6];
            }
        }

        public void ParseData(RawLogData rawData)
        {
            if (this.IsComplete)
            {
                throw new InvalidOperationException("This log row is already complete.");
            }

            this.responseData[rawData.Dpid] = rawData.Payload;

            if (rawData.Dpid == this.lastId)
            {
                this.IsComplete = true;
            }
        }

        public string[] Evaluate()
        {
            List<string> result = new List<string>();
            foreach(ParameterGroup group in this.profile.ParameterGroups)
            {
                byte[] payload;
                if (!this.responseData.TryGetValue((byte)group.Dpid, out payload))
                {
                    foreach(Parameter parameter in group.Parameters)
                    {
                        result.Add(string.Empty);
                    }

                    continue;
                }

                this.EvaluateGroupPayload(group, payload, result);
            }

            return result.ToArray();
        }

        private void EvaluateGroupPayload(ParameterGroup group, byte[] payload, List<string> result)
        {
            int startIndex = 0;
            foreach(ProfileParameter parameter in group.Parameters)
            {
                int value;
                switch(parameter.ByteCount)
                {
                    case 1:
                        value = payload[startIndex++];
                        break;

                    case 2:
                        value = payload[startIndex++];
                        value <<= 8;
                        value |= payload[startIndex++];
                        break;

                    default:
                        throw new InvalidOperationException("ByteCount must be 1 or 2");
                }

                Interpreter interpreter = new Interpreter();
                interpreter.SetVariable("x", value);
                double converted = interpreter.Eval<double>(parameter.Conversion.Expression);

                string format = parameter.Conversion.Format;
                if (string.IsNullOrWhiteSpace(format))
                {
                    format = "0.00";
                }

                string strung = converted.ToString(format);

                result.Add(strung);
            }
        }
    }

    public class Logger
    {
        private Vehicle vehicle;
        private LogProfile profile;
        private byte[] dpids;

#if FAST_LOGGING
        private DateTime lastRequestTime;
#endif

        public Logger(Vehicle vehicle, LogProfile profile)
        {
            this.vehicle = vehicle;
            this.profile = profile;
        }

        public async Task<bool> StartLogging()
        {
            this.dpids = await this.vehicle.ConfigureDpids(this.profile);

            if (this.dpids == null)
            {
                return false;
            }

#if FAST_LOGGING
            if (!await this.vehicle.RequestDpids(this.dpids))
            {
                return null;
            }

            this.lastRequestTime = DateTime.Now;
#endif
            return true;
        }

        public async Task<string[]> GetNextRow()
        {
            LogRowParser row = new LogRowParser(this.profile);

#if FAST_LOGGING
            if (DateTime.Now.Subtract(lastRequestTime) > TimeSpan.FromSeconds(2))
            {
                await this.vehicle.ForceSendToolPresentNotification();
            }
#endif

            while (!row.IsComplete)
            {
#if !FAST_LOGGING
                if (!await this.vehicle.RequestDpids(this.dpids))
                {
                    return null;
                }
#endif

                RawLogData rawData = await this.vehicle.ReadLogData();
                if (rawData == null)
                {
                    return null;
                }

                row.ParseData(rawData);
            }

            return row.Evaluate();
        }
    }
}
