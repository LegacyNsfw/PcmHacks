using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// Writes .csv files of log data.
    /// </summary>
    public class LogFileWriter
    {
        private StreamWriter writer;
        private DateTime startTime;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LogFileWriter(StreamWriter writer)
        {
            this.writer = writer;
        }

        /// <summary>
        /// Call this once to write the file header.
        /// </summary>
        public async Task WriteHeader(LogProfile profile)
        {
            this.startTime = DateTime.Now;
            string text = profile.GetParameterNames(", ");
            await this.writer.WriteAsync("Clock Time, Elapsed Time, ");
            await this.writer.WriteLineAsync(text);
        }

        /// <summary>
        /// Call this to write each new row to the file.
        /// </summary>
        public async Task WriteLine(string[] values)
        {
            await this.writer.WriteAsync(DateTime.Now.ToString("u"));
            await this.writer.WriteAsync(", ");
            await this.writer.WriteAsync(DateTime.Now.Subtract(this.startTime).ToString());
            await this.writer.WriteAsync(", ");
            await this.writer.WriteLineAsync(string.Join(", ", values));
        }
    }
}
