using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// The Interface classes are responsible for commanding a hardware
    /// interface to send and receive VPW messages.
    /// 
    /// They use the Protocol class to generate and interpret VPW messages.
    /// 
    /// They use the IPort interface to communicate with the hardware.
    /// </summary>
    abstract class Interface : IDisposable
    {
        protected IPort Port { get; private set; }

        public Interface(IPort port)
        {
            this.Port = port;
        }

        ~Interface()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.ClosePort();
        }

        public virtual async Task OpenPort()
        {
            await this.Port.Open();
        }

        protected virtual void ClosePort()
        {
            if (this.Port != null)
            {
                this.Port.Dispose();
            }
        }

        public abstract Task<string> QueryVin();

        public abstract Task<string> QueryOS();

        public abstract Task<int> QuerySeed();

        public abstract Task<bool> SendKey(int key);

        public abstract Task<bool> SendKernel();

        public abstract Task<Stream> ReadContents();
    }
}
