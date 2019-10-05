using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using DynamicExpresso;

namespace PcmHacking
{
    /// <summary>
    /// Reads bytes from the PCM and turns them into readable strings.
    /// </summary>
    public class LogRowParser
    {
        private LogProfile profile;
        private Dictionary<byte, byte[]> responseData = new Dictionary<byte, byte[]>();
        private HashSet<byte> dpidsReceived = new HashSet<byte>();
        private int dpidsInProfile;

        public bool IsComplete { get { return this.dpidsInProfile == this.dpidsReceived.Count; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        public LogRowParser(LogProfile profile)
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
        public string[] Evaluate()
        {
            List<string> result = new List<string>();
            foreach (ParameterGroup group in this.profile.ParameterGroups)
            {
                byte[] payload;
                if (!this.responseData.TryGetValue((byte)group.Dpid, out payload))
                {
                    foreach (Parameter parameter in group.Parameters)
                    {
                        result.Add(string.Empty);
                    }

                    continue;
                }

                this.EvaluateDpidMessage(group, payload, result);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Evaluates the payloads from a single dpid / group of parameters.
        /// </summary>
        private void EvaluateDpidMessage(ParameterGroup group, byte[] payload, List<string> result)
        {
            int startIndex = 0;
            foreach (ProfileParameter parameter in group.Parameters)
            {
                int value;
                switch (parameter.ByteCount)
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
}
