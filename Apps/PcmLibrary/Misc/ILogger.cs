using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// This interface allows other classes to send user-friendly status messages and
    /// developer-oriented debug messages to the UI.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Add a message to the 'results' pane of the UI.
        /// </summary>
        /// <remarks>
        /// These messages should be things that end users will understand.
        /// They should describe major operations, not sequences of bytes.
        /// </remarks>
        void AddUserMessage(string message);

        /// <summary>
        /// Add a message to the 'debug' pane of the UI.
        /// </summary>
        /// These should be things that we can use to diagnose errors.
        /// Feel free to include raw sequences of bytes.
        void AddDebugMessage(string message);

        /// <summary>
        /// Status Strip Updater's
        /// </summary>
        void StatusUpdateActivity(string activity);
        void StatusUpdateTimeRemaining(string remaining);
        void StatusUpdatePercentDone(string percent);
        void StatusUpdateRetryCount(string retries);
        void StatusUpdateProgressBar(double completed, bool visible);
        void StatusUpdateKbps(string Kbps);
        void StatusUpdateReset();
    }
}
