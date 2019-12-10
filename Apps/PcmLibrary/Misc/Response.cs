using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// These values indicate what went wrong when we were trying to recevie a response from the ECU.
    /// </summary>
    public enum ResponseStatus
    {
        /// <summary>
        /// Unspecified error type - try to avoid using this.
        /// </summary>
        Error = 0,

        /// <summary>
        /// Successful response.
        /// </summary>
        Success = 1,

        /// <summary>
        /// Response was shorter than expected.
        /// </summary>
        Truncated = 2,

        /// <summary>
        /// Response contained data that differs from what was expected.
        /// </summary>
        UnexpectedResponse = 3,

        /// <summary>
        /// No response was received before the timeout expired.
        /// </summary>
        Timeout = 4,

        /// <summary>
        /// The operation was cancelled by the user.
        /// </summary>
        Cancelled = 5,

        /// <summary>
        /// The request was refused.
        /// </summary>
        Refused = 6,
    }

    /// <summary>
    /// See the Response[T] class below. This one just contains Response-
    /// related methods that can be called without requiring explicit 
    /// generic parameters.
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Create a response with the given status and value.
        /// </summary>
        /// <remarks>
        /// This just makes the calling code simpler because you don't have to specify T explicitly.
        /// </remarks>
        public static Response<T> Create<T>(ResponseStatus status, T value, int retryCount = -1)
        {
            return new Response<T>(status, value, retryCount);
        }
    }

    /// <summary>
    /// Response objects contain response data, or an error status and placeholder data.
    /// </summary>
    /// <remarks>
    /// The idea here is to make it easy to communicate values and errors from 
    /// low-level code up to the UI, using a single object.
    /// </remarks>
    public class Response<T>
    {
        /// <summary>
        /// Indicates success or gives us some idea of what went wrong.
        /// </summary>
        public ResponseStatus Status { get; private set; }

        /// <summary>
        /// Indicates how many times the operation had to be retried.
        /// </summary>
        public int RetryCount { get; private set; }

        /// <summary>
        /// The value that came from the PCM.
        /// </summary>
        /// <remarks>
        /// Lower-level code operates on byte arrays, but higher level code can
        /// operate on strings or integers or other data types. 
        /// 
        /// If the Status property is not 'Success' then the value of this 
        /// property should be null, zero, empty string, etc.
        /// </remarks>
        public T Value { get; private set; }

        /// <summary>
        /// Create a Response object with the given status and value.
        /// </summary>
        public Response(ResponseStatus status, T value, int retryCount = -1)
        {
            this.Status = status;
            this.Value = value;
            this.RetryCount = retryCount;
        }

        /// <summary>
        /// This is the string that appears when you hover the mouse over something in the debugger.
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} {1}", this.Status, this.Value?.ToString());
        }
    }
}
