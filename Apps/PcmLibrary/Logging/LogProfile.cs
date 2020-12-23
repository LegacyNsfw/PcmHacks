using System;
using System.Collections.Generic;
using System.Text;

namespace PcmHacking
{
    public class LogProfile
    {
        private List<ProfileParameter> parameters;

        public IEnumerable<ProfileParameter> Parameters { get { return this.parameters; } }

        public bool IsEmpty { get { return this.parameters.Count == 0; } }

        public LogProfile()
        {
            this.parameters = new List<ProfileParameter>();
        }

        public void AddParameter(ProfileParameter parameter)
        {
            this.parameters.Add(parameter);
        }

        public void RemoveParameter(ProfileParameter parameter)
        {
            this.parameters.Remove(parameter);
        }
    }
}
