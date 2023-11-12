using System;
using System.Collections.Generic;
using System.Text;

namespace PcmHacking
{
    public class CanLogger
    {
        private IPort canPort;
        public CanLogger(IPort canPort)
        {
            this.canPort = canPort; 
        }

        /// <summary>
        /// Create a background thread that will receive data from the CAN port.
        /// </summary>
        public void Start()
        {
    

        }

        public void Stop()
        {

        }
    }
}
