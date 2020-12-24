using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PcmHacking
{    
    public class ParameterGroup
    {
        public const int MaxBytes = 6;

        public UnsignedHexValue Dpid { get; set; }

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

        public List<ProfileParameter> Parameters { get; set; }

        public ParameterGroup(byte id)
        {
            this.Dpid = new UnsignedHexValue(id);
            this.Parameters = new List<ProfileParameter>();
        }

        public int TotalBytes
        {
            get
            {
                return Parameters.Sum(x => (x.Parameter as PcmParameter).ByteCount);
            }
        }

        public override string ToString()
        {
            return string.Join(", ", new string[] { this.Dpid.ToString() }.Concat(this.Parameters.Select(x => x.Parameter.Name)));
        }

        public bool TryAddParameter(ProfileParameter parameter)
        {
            if (this.TotalBytes + (parameter.Parameter as PcmParameter).ByteCount > MaxBytes)
            {
                return false;
            }

            this.Parameters.Add(parameter);
            return true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    //[DataContract]
    public class DpidConfiguration
    {
        public const int MaxGroups = 3;

        public List<ParameterGroup> ParameterGroups { get; set; }

        public int ParameterCount
        {
            get
            {
                return this.ParameterGroups.Sum(x => x.Parameters.Count);
            }
        }

        public IList<ProfileParameter> AllParameters
        {
            get
            {
                List<ProfileParameter> allParameters = new List<ProfileParameter>();
                this.ParameterGroups
                    .ForEach(group => group.Parameters
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
                    group => group.Parameters.Select(
                        parameter => string.Format("{0} ({1})", parameter.Parameter.Name, parameter.Conversion.Units)));
        }

        public override string ToString()
        {
            return string.Format("{0} parameters", this.AllParameters.Count);
        }
    }
}
