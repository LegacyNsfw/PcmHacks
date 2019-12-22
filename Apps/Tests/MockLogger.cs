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
    }
}
