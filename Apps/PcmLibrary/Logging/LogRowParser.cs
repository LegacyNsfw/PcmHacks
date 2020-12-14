using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DynamicExpresso;

namespace PcmHacking
{
    public class ParameterValue
    {
        public double RawValue { get; set; }
        public string ValueAsString { get; set; }
        public double ValueAsDouble { get; set; }
    }

    public class DpidValues : Dictionary<ProfileParameter, ParameterValue>
    {
    }

    /// <summary>
    /// Reads bytes from the PCM and turns them into readable strings.
    /// </summary>
    public class LogRowParser
    {
        private DpidConfiguration profile;
        private Dictionary<byte, byte[]> responseData = new Dictionary<byte, byte[]>();
        private HashSet<byte> dpidsReceived = new HashSet<byte>();
        private int dpidsInProfile;
        
        public bool IsComplete { get { return this.dpidsInProfile == this.dpidsReceived.Count; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        public LogRowParser(DpidConfiguration profile)
        {
            this.profile = profile;

            // Create a place to put response data.
            foreach (ParameterGroup group in this.profile.ParameterGroups)
            {
                responseData[(byte)group.Dpid] = new byte[6];
            }

            this.dpidsInProfile = this.profile.ParameterGroups.Count;
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
        public DpidValues Evaluate()
        {
            DpidValues results = new DpidValues();
            foreach (ParameterGroup group in this.profile.ParameterGroups)
            {
                byte[] payload;
                if (!this.responseData.TryGetValue((byte)group.Dpid, out payload))
                {
                    foreach (ProfileParameter parameter in group.Parameters)
                    {
                        results.Add(parameter, new ParameterValue() { ValueAsString = string.Empty, ValueAsDouble = 0 });
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
        private void EvaluateDpidMessage(ParameterGroup group, byte[] payload, DpidValues results)
        {
            int startIndex = 0;
            foreach (ProfileParameter parameter in group.Parameters)
            {
                Int16 value = 0;
                switch (parameter.Parameter.ByteCount)
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

                if (parameter.Conversion.Expression == "0x")
                {
                    string format = parameter.Parameter.ByteCount == 1 ? "X2" : "X4";

                    results.Add(
                        parameter,
                        new ParameterValue()
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
                    double convertedValue = interpreter.Eval<double>(parameter.Conversion.Expression);

                    string format = parameter.Conversion.Format;
                    if (string.IsNullOrWhiteSpace(format))
                    {
                        format = "0.00";
                    }

                    string formatted = convertedValue.ToString(format);

                    results.Add(
                        parameter,
                        new ParameterValue()
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
