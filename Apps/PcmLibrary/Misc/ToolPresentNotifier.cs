using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    public class ToolPresentNotifier
    {
        ILogger logger;
        MessageFactory messageFactory;
        Device device;
        DateTime lastNotificationTime = DateTime.MinValue;

        public ToolPresentNotifier(ILogger logger, MessageFactory messageFactory, Device device)
        {
            this.logger = logger;
            this.messageFactory = messageFactory;
            this.device = device;
        }

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

        private async Task SendNotification()
        {
            this.logger.AddDebugMessage("Sending 'test device present' notification.");
            Message message = this.messageFactory.CreateTestDevicePresent();
            await this.device.SendMessage(message);
        }
    }
}
