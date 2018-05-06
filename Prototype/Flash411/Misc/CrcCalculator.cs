using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    public class CrcCalculator
    {
        public int Result { get; private set; }

        public CrcCalculator()
        {
            this.Result = 0xFF;
        }

        public void Add(byte value)
        {
            this.Result ^= value;
            int b;
            for (b = 0; b < 8; b++)
            {
                if ((this.Result & 0x80) != 0)
                {
                    this.Result <<= 1;
                    this.Result ^= 0x11D;
                }
                else
                {
                    this.Result <<= 1;
                }
            }
        }
    }
}
