using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PcmHacking
{
    public class LogProfileAndMath
    {
        private readonly LogProfile profile;
        private readonly MathValueConfiguration mathValueConfiguration;
        private readonly MathValueProcessor mathValueProcessor;

        public LogProfile Profile
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

        public LogProfileAndMath(LogProfile profile, MathValueConfiguration mathValueConfiguration)
        {
            this.profile = profile;
            this.mathValueConfiguration = mathValueConfiguration;
            this.mathValueProcessor = new MathValueProcessor(
                this.profile,
                this.mathValueConfiguration);
        }

        public IEnumerable<string> GetColumnNames()
        {
            return this.profile.GetParameterNames().Concat(this.mathValueProcessor.GetHeaders());
        }
    }
}
