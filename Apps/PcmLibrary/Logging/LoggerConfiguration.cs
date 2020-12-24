using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PcmHacking
{
    public class LoggerConfiguration
    {
        private readonly DpidConfiguration profile;
        private readonly MathValueProcessor mathValueProcessor;

        public DpidConfiguration Profile
        {
            get
            {
                return this.profile;
            }
        }

        public MathValueProcessor MathValueProcessor
        {
            get
            {
                return this.mathValueProcessor;
            }
        }

        public LoggerConfiguration(DpidConfiguration profile, MathValueConfiguration mathValueConfiguration)
        {
            this.profile = profile;
            this.mathValueProcessor = new MathValueProcessor(
                this.profile,
                mathValueConfiguration);
        }

        public IEnumerable<string> GetColumnNames()
        {
            return this.profile.GetParameterNames().Concat(this.mathValueProcessor.GetHeaders());
        }
    }
}
