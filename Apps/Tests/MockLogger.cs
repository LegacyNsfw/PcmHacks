using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcmHacking;

namespace Tests
{
    class MockLogger : ILogger
    {
        public void AddDebugMessage(string message)
        {            
        }

        public void AddUserMessage(string message)
        {
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
