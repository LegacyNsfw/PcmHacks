using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PcmHacking
{
    public class LogProfileWriter
    {
        private Stream stream;

        public LogProfileWriter(Stream stream)
        {
            this.stream = stream;
        }

        public async Task WriteAsync(LogProfile profile)
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                string json = JsonConvert.SerializeObject(profile, new UnsignedHexValueConverter());
                await writer.WriteLineAsync(json);
            }
        }
    }
}
