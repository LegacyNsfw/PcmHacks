using System;
using System.Collections.Generic;
using System.Text;

namespace PcmHacking
{
    /// <summary>
    /// For now, a log is just a list of columns.
    /// Hopefully we'll add dashboard layouts to this in the future.
    /// </summary>
    public class LogProfile
    {
        private List<LogColumn> columns;

        public IEnumerable<LogColumn> Columns { get { return this.columns; } }

        public bool IsEmpty { get { return this.columns.Count == 0; } }

        public LogProfile()
        {
            this.columns = new List<LogColumn>();
        }

        public void AddColumn(LogColumn logColumn)
        {
            this.columns.Add(logColumn);
        }

        public void RemoveColumn(LogColumn logColumn)
        {
            this.columns.Remove(logColumn);
        }
    }
}
