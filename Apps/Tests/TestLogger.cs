using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Flash411;

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
    }
}
