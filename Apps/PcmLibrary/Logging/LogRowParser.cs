using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DynamicExpresso;

namespace PcmHacking
{
    public class PcmParameterValue
    {
        public Int16 RawValue { get; set; }
        public string ValueAsString { get; set; }
        public double ValueAsDouble { get; set; }
    }

    public class PcmParameterValues : Dictionary<LogColumn, PcmParameterValue>
    {
    }

    /// <summary>
    /// Reads bytes from the PCM and turns them into readable strings.
    /// </summary>
    public class LogRowParser
    {
        private DpidConfiguration dpidConfiguration;
        private Dictionary<byte, byte[]> responseData = new Dictionary<byte, byte[]>();
        private HashSet<byte> dpidsReceived = new HashSet<byte>();
        private int dpidCount;
        
        public bool IsComplete { get { return this.dpidCount == this.dpidsReceived.Count; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        public LogRowParser(DpidConfiguration profile)
        {
            this.dpidConfiguration = profile;

            // Create a place to put response data.
            foreach (ParameterGroup group in this.dpidConfiguration.ParameterGroups)
            {
                responseData[(byte)group.Dpid] = new byte[6];
            }

            this.dpidCount = this.dpidConfiguration.ParameterGroups.Count;
        }

        /// <summary>
        /// Extracts the payload from a dpid message from the PCM.
        /// </summary>
        /// <param name="rawData"></param>
        public void ParseData(RawLogData rawData)
        {
            if (this.IsComplete)
            {
                throw new InvalidOperationException("This log row is already complete.");
            }

            this.responseData[rawData.Dpid] = rawData.Payload;
            this.dpidsReceived.Add(rawData.Dpid);
        }

        /// <summary>
        /// Evalutes all of the dpid payloads.
        /// </summary>
        /// <returns></returns>
        public PcmParameterValues Evaluate()
        {
            PcmParameterValues results = new PcmParameterValues();
            foreach (ParameterGroup group in this.dpidConfiguration.ParameterGroups)
            {
                byte[] payload;
                if (!this.responseData.TryGetValue((byte)group.Dpid, out payload))
                {
                    foreach (LogColumn column in group.LogColumns)
                    {
                        results.Add(column, new PcmParameterValue() { ValueAsString = string.Empty, ValueAsDouble = 0 });
                    }

                    continue;
                }

                this.EvaluateDpidMessage(group, payload, results);
            }

            return results;
        }

        /// <summary>
        /// Evaluates the payloads from a single dpid / group of parameters.
        /// </summary>
        private void EvaluateDpidMessage(ParameterGroup group, byte[] payload, PcmParameterValues results)
        {
            int startIndex = 0;
            foreach (LogColumn column in group.LogColumns)
            {
                Int16 value = 0;
                PcmParameter pcmParameter = column.Parameter as PcmParameter;
                if (pcmParameter == null)
                {
                    continue;
                }

                switch (pcmParameter.ByteCount)
                {
                    case 1:
                        if (startIndex < payload.Length)
                        {
                            value = payload[startIndex++];
                        }
                        break;

                    case 2:
                        if (startIndex < payload.Length)
                        {
                            value = payload[startIndex++];
                        }

                        value <<= 8;

                        if (startIndex < payload.Length)
                        {
                            value = (Int16)((UInt16)value | (byte)payload[startIndex++]);
                        }
                        break;

                    default:
                        throw new InvalidOperationException("ByteCount must be 1 or 2");
                }

                if (column.Conversion.Expression == "0x")
                {
                    string format = pcmParameter.ByteCount == 1 ? "X2" : "X4";

                    results.Add(
                        column,
                        new PcmParameterValue()
                        {
                            RawValue = value,
                            ValueAsDouble = value,
                            ValueAsString = value.ToString(format)
                        });
                }
                else
                {
                    Interpreter interpreter = new Interpreter();
                    interpreter.SetVariable("x", value);
                    interpreter.SetVariable("x_high", value >> 8);
                    interpreter.SetVariable("x_low", value & 0xFF);

                    double convertedValue = interpreter.Eval<double>(column.Conversion.Expression);

                    string format = column.Conversion.Format;
                    if (string.IsNullOrWhiteSpace(format))
                    {
                        format = "0.00";
                    }

                    string formatted = convertedValue.ToString(format);

                    results.Add(
                        column,
                        new PcmParameterValue()
                        {
                            RawValue = value,
                            ValueAsDouble = convertedValue,
                            ValueAsString = formatted
                        });
                }
            }
        }
    }
}
