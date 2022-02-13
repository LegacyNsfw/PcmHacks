using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DynamicExpresso;

namespace PcmHacking
{
    /// <summary>
    /// Combines the raw value read from the PCM with string and double representations.
    /// </summary>
    public class PcmParameterValue
    {
        public string ValueAsString { get; set; }
        public double ValueAsDouble { get; set; }
    }

    /// <summary>
    /// Maps log columns to values.
    /// </summary>
    public class PcmParameterValues : Dictionary<LogColumn, PcmParameterValue>
    {
    }

    /// <summary>
    /// Reads bytes from the PCM and turns them into doubles and readable strings.
    /// </summary>
    public class LogRowParser
    {
        private DpidConfiguration dpidConfiguration;
        private Dictionary<byte, byte[]> responseData = new Dictionary<byte, byte[]>();
        private HashSet<byte> dpidsReceived = new HashSet<byte>();
        private int dpidCount;
        
        public bool IsComplete
        {
            get
            {
                foreach(var group in this.dpidConfiguration.ParameterGroups)
                {
                    if (!this.dpidsReceived.Contains(group.Dpid))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

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
                double value;
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
                            if (pcmParameter.IsSigned)
                            {
                                value = (sbyte)payload[startIndex++];
                            }
                            else
                            {
                                value = (byte)payload[startIndex++];
                            }                            
                        }
                        else
                        {
                            value = 0;
                        }

                        break;

                    case 2:
                        if (startIndex + 1 < payload.Length)
                        {
                            if (pcmParameter.IsSigned)
                            {
                                Int16 temp = (Int16)(payload[startIndex] << 8);
                                temp += (byte)payload[startIndex + 1];
                                value = temp;
                            }
                            else
                            {
                                UInt16 temp = (UInt16)(payload[startIndex] << 8);
                                temp += payload[startIndex + 1];
                                value = temp;
                            }

                            startIndex += 2;
                        }
                        else
                        {
                            value = 0;
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
                            ValueAsDouble = value,
                            ValueAsString = value.ToString(format)
                        });
                }
                else
                {
                    double convertedValue = 0;
                    string formattedValue;

                    if (column.Conversion.IsBitMapped)
                    {
                        int bits = (int)value;
                        bits = bits >> column.Conversion.BitIndex;
                        bool flag = (bits & 1) != 0;

                        convertedValue = value;
                        formattedValue = flag ? column.Conversion.TrueValue : column.Conversion.FalseValue;
                    }
                    else
                    {
                        try
                        {
                            Interpreter interpreter = new Interpreter();
                            interpreter.SetVariable("x", value);

                            convertedValue = interpreter.Eval<double>(column.Conversion.Expression);
                        }
                        catch (Exception exception)
                        {
                            throw new InvalidOperationException(
                                string.Format("Unable to evaluate expression \"{0}\" for parameter \"{1}\"",
                                    column.Conversion.Expression,
                                    column.Parameter.Name),
                                exception);
                        }

                        string format = column.Conversion.Format;
                        if (string.IsNullOrWhiteSpace(format))
                        {
                            format = "0.00";
                        }

                        formattedValue = convertedValue.ToString(format);
                    }

                    results.Add(
                        column,
                        new PcmParameterValue()
                        {
                            ValueAsDouble = convertedValue,
                            ValueAsString = formattedValue
                        });
                }
            }
        }
    }
}
