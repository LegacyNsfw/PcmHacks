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
        public async Task WriteHeader(IEnumerable<string> columnNames)
        {
            this.startTime = DateTime.Now;
            string text = string.Join(", ", columnNames);  
            await this.writer.WriteAsync("Clock Time, Elapsed Time, ");
            await this.writer.WriteLineAsync(text);
        }

        /// <summary>
        /// Call this to write each new row to the file.
        /// </summary>
        public void WriteLine(IEnumerable<string> values)
        {
            lock (this.writer)
            {
                this.writer.Write(DateTime.Now.ToString("u"));
                this.writer.Write(", ");
                this.writer.Write(DateTime.Now.Subtract(this.startTime).ToString());
                this.writer.Write(", ");
                this.writer.WriteLine(string.Join(", ", values));
            }
        }
    }
}
