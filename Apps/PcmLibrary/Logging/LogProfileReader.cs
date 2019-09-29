using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PcmHacking
{
    public class LogProfileReader
    {
        private Stream stream;

        public LogProfileReader(Stream stream)
        {
            this.stream = stream;
        }

        public async Task<LogProfile> ReadAsync()
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string json = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<LogProfile>(json, new UnsignedHexValueConverter());
            }
        }
    }
}
