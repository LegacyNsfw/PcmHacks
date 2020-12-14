using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PcmHacking
{
    public class DpidConfigurationWriter
    {
        private Stream stream;

        public DpidConfigurationWriter(Stream stream)
        {
            this.stream = stream;
        }

        public async Task WriteAsync(DpidConfiguration profile)
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                string json = JsonConvert.SerializeObject(profile, new UnsignedHexValueConverter());
                await writer.WriteLineAsync(json);
            }
        }
    }
}
