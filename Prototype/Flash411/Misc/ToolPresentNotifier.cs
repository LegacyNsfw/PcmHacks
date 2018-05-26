using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    class ToolPresentNotifier
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
            if(DateTime.Now > lastNotificationTime + TimeSpan.FromSeconds(2))
            {
                await this.SendNotification();
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
