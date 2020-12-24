using System;
using System.Collections.Generic;
using System.Text;

namespace PcmHacking
{
    public class LogColumn
    {
        public Parameter Parameter { get; private set; }
        public Conversion Conversion { get; private set; }

        public LogColumn(Parameter parameter, Conversion conversion)
        {
            this.Parameter = parameter;
            this.Conversion = conversion;
        }

        public override string ToString()
        {
            return base.ToString() + ", " + this.Conversion.ToString();
        }
    }
}
