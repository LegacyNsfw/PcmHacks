using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// Send VPW "tool present" messages to keep the PCM in a state receptive to reading and writing.
    /// </summary>
    public class ToolPresentNotifier
    {
        /// <summary>
        /// Provides access to the Results and Debug panes.
        /// </summary>
        ILogger logger;

        /// <summary>
        /// Generates VPW messages.
        /// </summary>
        Protocol protocol;

        /// <summary>
        /// The device to send messages with.
        /// </summary>
        Device device;

        /// <summary>
        /// When the last message was sent.
        /// </summary>
        DateTime lastNotificationTime = DateTime.MinValue;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ToolPresentNotifier(Device device, Protocol protocol, ILogger logger)
        {
            this.logger = logger;
            this.protocol = protocol;
            this.device = device;
        }

        /// <summary>
        /// Send a tool-present message, if the time is right.
        /// </summary>
        /// <returns></returns>
        public async Task Notify()
        {
            // Tool present / 3F is required every 2.5 seconds.
            //
            // This timer ensures we won't call it more often than every 2 seconds,
            // but there is no upper bound because other code could spend lots of 
            // time between calls to this code. 
            //
            // Consider reducing this to 1.5 seconds if 2 seconds isn't fast enough.
            if(DateTime.Now > this.lastNotificationTime + TimeSpan.FromSeconds(2))
            {
                await this.SendNotification();
                this.lastNotificationTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Send a tool-present message, even if not much time has passed. This is to aid in polling.
        /// </summary>
        /// <returns></returns>
        public async Task ForceNotify()
        {
            await this.SendNotification();
        }

        /// <summary>
        /// Send a tool-present message.
        /// </summary>
        private async Task SendNotification()
        {
            this.logger.AddDebugMessage("Sending 'test device present' notification.");
            Message message = this.protocol.CreateTestDevicePresentNotification();
            TimeoutScenario originalScenario = await this.device.SetTimeout(TimeoutScenario.Minimum);
            await this.device.SendMessage(message);
            await this.device.SetTimeout(originalScenario);
        }
    }
}
