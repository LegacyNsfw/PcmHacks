using System;
using System.Collections.Generic;
using System.Text;

namespace PcmHacking
{
    public class DataTruncatedException : Exception
    {
        public DataTruncatedException(string message = "Data Truncated"): base (message)
        {
        }
    }

    public class UnsupportedFormatException: Exception
    {
        public UnsupportedFormatException(string message = "Data format not supported."): base(message)
        {
        }
    }
}
