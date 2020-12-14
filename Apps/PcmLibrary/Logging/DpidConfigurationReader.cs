using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PcmHacking
{
    public class DpidConfigurationReader
    {
        private Stream stream;

        public DpidConfigurationReader(Stream stream)
        {
            this.stream = stream;
        }

        public async Task<DpidConfiguration> ReadAsync()
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string json = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<DpidConfiguration>(json, new UnsignedHexValueConverter());
            }
        }
    }
}
