using System;
using System.Collections.Generic;
using System.Text;

namespace PcmHacking
{
    /// <summary>
    /// Each column in the data log is defined by the parameter and how
    /// the raw parameter value is converted to the preferred units.
    /// </summary>
    public class LogColumn
    {
        /// <summary>
        /// Which parameter to fetch from the PCM.
        /// </summary>
        public Parameter Parameter { get; private set; }

        /// <summary>
        /// How to convert that parameter value to usable units.
        /// </summary>
        public Conversion Conversion { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public LogColumn(Parameter parameter, Conversion conversion)
        {
            this.Parameter = parameter;
            this.Conversion = conversion;
        }

        /// <summary>
        /// Make it readable in the debugger.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Parameter.ToString() + ", " + this.Conversion.ToString();
        }
    }
}
