using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    interface ILogger
    {
        void AddUserMessage(string message);
        void AddDebugMessage(string message);
    }
}
