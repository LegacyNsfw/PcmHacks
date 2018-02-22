using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// This class encapsulates all code that is unique to the Arduino-based interface that Thaniel has created.
    /// </summary>
    class ThanielInterface : Interface
    {
        public ThanielInterface(IPort port) : base (port)
        {

        }

        public override string ToString()
        {
            return "Thaniel 1.0";
        }

        public override Task<string> QueryVin()
        {
            throw new NotImplementedException();
        }

        public override Task<string> QueryOS()
        {
            throw new NotImplementedException();
        }

        public override Task<int> QuerySeed()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> SendKey(int key)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> SendKernel()
        {
            throw new NotImplementedException();
        }

        public override Task<Stream> ReadContents()
        {
            throw new NotImplementedException();
        }
    }
}
