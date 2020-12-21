using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PcmHacking
{
    public class DpidsAndMath
    {
        private readonly DpidConfiguration profile;
        private readonly MathValueConfiguration mathValueConfiguration;
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

        public DpidsAndMath(DpidConfiguration profile, MathValueConfiguration mathValueConfiguration)
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
