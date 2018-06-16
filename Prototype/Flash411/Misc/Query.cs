using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    class Query<T>
    {
        private Device device;
        private Func<Message> generator;
        private Func<Message, Response<T>> filter;
        private ILogger logger;

        public Query(Device device, Func<Message> generator, Func<Message, Response<T>> filter, ILogger logger)
        {
            this.device = device;
            this.generator = generator;
            this.filter = filter;
            this.logger = logger;
        }

        public async Task<Response<T>> Execute()
        {
            this.device.ClearMessageQueue();

            Message request = this.generator();

            bool success = false;
            for (int sendAttempt = 1; sendAttempt <= 5; sendAttempt++)
            {
                success = await this.device.SendMessage(request);

                if (!success)
                {
                    this.logger.AddDebugMessage("Send failed. Attempt #" + sendAttempt.ToString());
                    continue;
                }

                // We'll read up to 50 times from the queue (just to avoid 
                // looping forever) but we will but only allow two timeouts.
                int timeouts = 0;
                for (int receiveAttempt = 1; receiveAttempt <= 50; receiveAttempt++)
                {
                    Message received = await this.device.ReceiveMessage();

                    if (received == null)
                    {
                        timeouts++;
                        if (timeouts >= 2)
                        {
                            // Maybe try sending again if we haven't run out of send attempts.
                            this.logger.AddDebugMessage(
                                string.Format(
                                    "Receive timed out. Attempt #{0}, Timeout #{1}.",
                                    receiveAttempt,
                                    timeouts));
                            break;
                        }
                    }

                    Response<T> result = this.filter(received);
                    if (result.Status == ResponseStatus.Success)
                    {
                        return result;
                    }

                    this.logger.AddDebugMessage(
                        string.Format(
                            "Received an unexpected response. Attempt #{0}, status {1}.",
                            receiveAttempt,
                            result.Status));
                }
            }

            return Response.Create(ResponseStatus.Error, default(T));
        }
    }
}
