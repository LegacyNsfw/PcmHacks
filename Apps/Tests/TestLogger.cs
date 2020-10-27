using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PcmHacking;

namespace Tests
{
    class TestLogger : ILogger
    {
        public StringBuilder UserMessages { get; private set; }
        public StringBuilder DebugMessages { get; private set; }

        public TestLogger()
        {
            this.UserMessages = new StringBuilder();
            this.DebugMessages = new StringBuilder();
        }

        public void AddDebugMessage(string message)
        {
            this.DebugMessages.AppendLine(message);
        }

        public void AddUserMessage(string message)
        {
            this.UserMessages.AppendLine(message);
        }

        public void StatusUpdateActivity(string activity)
        {
        }

        public void StatusUpdateTimeRemaining(string remaining)
        {
        }

        public void StatusUpdatePercentDone(string percent)
        {
        }

        public void StatusUpdateRetryCount(string retries)
        {
        }

        public void StatusUpdateProgressBar(double completed, bool visible)
        {
        }

        public void StatusUpdateKbps(string Kbps)
        {
        }

        public void StatusUpdateReset()
        {
        }
    }
}
