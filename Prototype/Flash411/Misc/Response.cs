using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
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
    }

    /// <summary>
    /// See the Response`1 class below. This one just contains Response-
    /// related methods that without requiring explicit generic parameters.
    /// </summary>
    class Response
    {
        /// <summary>
        /// Create a response with the given status and value.
        /// </summary>
        /// <remarks>
        /// This just makes the calling code simpler because you don't have to specify T explicitly.
        /// </remarks>
        public static Response<T> Create<T>(ResponseStatus status, T value)
        {
            return new Response<T>(status, value);
        }
    }

    /// <summary>
    /// Response objects contain response data, or an error status and placeholder data.
    /// </summary>
    class Response<T>
    {
        public ResponseStatus Status { get; private set; }

        public T Value { get; private set; }

        public Response(ResponseStatus status, T value)
        {
            this.Status = status;
            this.Value = value;
        }
    }
}
