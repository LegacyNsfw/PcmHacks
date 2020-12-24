using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PcmHacking
{
    /// <summary>
    /// This links the DPID ID with the log columns whose data is in that DPID.
    /// </summary>
    public class ParameterGroup
    {
        public const int MaxBytes = 6;

        public byte Dpid { get; set; }

        public string DpidAttributeValue
        {
            get
            {
                return UnsignedHex.GetUnsignedHex((UInt32)this.Dpid);
            }

            set
            {
                this.Dpid = (byte)UnsignedHex.GetUnsignedHex(value);
            }
        }

        public List<LogColumn> LogColumns { get; set; }

        public ParameterGroup(byte id)
        {
            this.Dpid = id;
            this.LogColumns = new List<LogColumn>();
        }

        public int TotalBytes
        {
            get
            {
                return LogColumns.Sum(x => (x.Parameter as PcmParameter).ByteCount);
            }
        }

        public override string ToString()
        {
            return string.Join(", ", new string[] { this.Dpid.ToString() }.Concat(this.LogColumns.Select(x => x.Parameter.Name)));
        }

        public bool TryAddLogColumn(LogColumn logColumn)
        {
            if (this.TotalBytes + (logColumn.Parameter as PcmParameter).ByteCount > MaxBytes)
            {
                return false;
            }

            this.LogColumns.Add(logColumn);
            return true;
        }
    }

    /// <summary>
    /// This combines a set of ParameterGroups and simplifies getting the columns and their names.
    /// </summary>
    public class DpidConfiguration
    {
        // DPID numbers 0xF2-0xFE all work
        // 0xFE is highest priority
        // 0xFA is very slow
        public const int MaxGroups = 3;

        public List<ParameterGroup> ParameterGroups { get; set; }

        public int LogColumnCount
        {
            get
            {
                return this.ParameterGroups.Sum(x => x.LogColumns.Count);
            }
        }

        public IList<LogColumn> AllLogColumns
        {
            get
            {
                List<LogColumn> allParameters = new List<LogColumn>();
                this.ParameterGroups
                    .ForEach(group => group.LogColumns
                    .ForEach(parameter => allParameters.Add(parameter)));
                return allParameters;
            }
        }

        public DpidConfiguration()
        {
            this.ParameterGroups = new List<ParameterGroup>();
        }

        public bool TryAddGroup(ParameterGroup group)
        {
            if (this.ParameterGroups.Count + 1 > MaxGroups)
            {
                return false;
            }

            this.ParameterGroups.Add(group);
            return true;
        }

        public IEnumerable<string> GetParameterNames()
        {
            return this.ParameterGroups.SelectMany(
                    group => group.LogColumns.Select(
                        parameter => string.Format("{0} ({1})", parameter.Parameter.Name, parameter.Conversion.Units)));
        }

        public override string ToString()
        {
            return string.Format("{0} parameters", this.AllLogColumns.Count);
        }
    }
}
