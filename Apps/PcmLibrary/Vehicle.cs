using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// <summary>
    /// From the application's perspective, this class is the API to the vehicle.
    /// </summary>
    /// <remarks>
    /// Methods in this class are high-level operations like "get the VIN," or "read the contents of the EEPROM."
    /// </remarks>
    public partial class Vehicle : IDisposable
    {
        /// <summary>
        /// How many times we should attempt to send a message before giving up.
        /// </summary>
        private const int MaxSendAttempts = 10;

        /// <summary>
        /// How many times we should attempt to receive a message before giving up.
        /// </summary>
        /// <remarks>
        /// 10 is too small for the case when we get a bunch of "chatter 
        /// suppressed" messages right before trying to upload the kernel.
        /// Might be worth making this a parameter to the retry loops since
        /// in most cases when only need about 5.
        /// </remarks>
        private const int MaxReceiveAttempts = 15;

        /// <summary>
        /// The device we'll use to talk to the PCM.
        /// </summary>
        private Device device;

        /// <summary>
        /// This class knows how to generate message to send to the PCM.
        /// </summary>
        private MessageFactory messageFactory;

        /// <summary>
        /// This class knows how to parse the messages that come in from the PCM.
        /// </summary>
        private MessageParser messageParser;
        
        /// <summary>
        /// This is how we send user-friendly status messages and developer-oriented debug messages to the UI.
        /// </summary>
        private ILogger logger;

        public string DeviceDescription
        {
            get
            {
                return this.device.ToString();
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Vehicle(
            Device device, 
            MessageFactory messageFactory,
            MessageParser messageParser,
            ILogger logger)
        {
            this.device = device;
            this.messageFactory = messageFactory;
            this.messageParser = messageParser;
            this.logger = logger;
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~Vehicle()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Implements IDisposable.Dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Part of the Dispose pattern.
        /// </summary>
        protected void Dispose(bool isDisposing)
        {
            if (this.device != null)
            {
                this.device.Dispose();
                this.device = null;
            }
        }

        /// <summary>
        /// Re-initialize the device.
        /// </summary>
        public async Task<bool> ResetConnection()
        {
            return await this.device.Initialize();
        }

        /// <summary>
        /// Note that this has only been confirmed to work with ObdLink ScanTool devices.
        /// AllPro doesn't get the reply for some reason.
        /// Might work with AVT or J-tool, that hasn't been tested.
        /// </summary>
        public async Task<bool> IsInRecoveryMode()
        {
            this.device.ClearMessageQueue();

            for (int iterations = 0; iterations < 3; iterations++)
            {
                await this.TrySendMessage(new Message(new byte[] { 0x6C, 0x10, 0xF0, 0x62 }), "recovery query", 2);
                Message response = await this.device.ReceiveMessage();
                if (response == null)
                {
                    continue;
                }

                if (this.messageParser.ParseRecoveryModeBroadcast(response).Value == true)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Unlock the PCM by requesting a 'seed' and then sending the corresponding 'key' value.
        /// </summary>
        public async Task<bool> UnlockEcu(int keyAlgorithm)
        {
            await this.device.SetTimeout(TimeoutScenario.ReadProperty);

            this.device.ClearMessageQueue();

            this.logger.AddDebugMessage("Sending seed request.");
            Message seedRequest = this.messageFactory.CreateSeedRequest();

            if (!await this.TrySendMessage(seedRequest, "seed request"))
            {
                this.logger.AddUserMessage("Unable to send seed request.");
                return false;
            }

            bool seedReceived = false;
            UInt16 seedValue = 0;

            for (int attempt = 1; attempt < MaxReceiveAttempts; attempt++)
            {
                Message seedResponse = await this.device.ReceiveMessage();
                if (seedResponse == null)
                {
                    this.logger.AddDebugMessage("No response to seed request.");
                    return false;
                }

                if (this.messageParser.IsUnlocked(seedResponse.GetBytes()))
                {
                    this.logger.AddUserMessage("PCM is already unlocked");
                    return true;
                }

                this.logger.AddDebugMessage("Parsing seed value.");
                Response<UInt16> seedValueResponse = this.messageParser.ParseSeed(seedResponse.GetBytes());
                if (seedValueResponse.Status == ResponseStatus.Success)
                {
                    seedValue = seedValueResponse.Value;
                    seedReceived = true;
                    break;
                }

                this.logger.AddDebugMessage("Unable to parse seed response. Attempt #" + attempt.ToString());
            }

            if (!seedReceived)
            {
                this.logger.AddUserMessage("No seed reponse received, unable to unlock PCM.");
                return false;
            }

            if (seedValue == 0x0000)
            {
                this.logger.AddUserMessage("PCM Unlock not required");
                return true;
            }

            UInt16 key = KeyAlgorithm.GetKey(keyAlgorithm, seedValue);

            this.logger.AddDebugMessage("Sending unlock request (" + seedValue.ToString("X4") + ", " + key.ToString("X4") + ")");
            Message unlockRequest = this.messageFactory.CreateUnlockRequest(key);
            if (!await this.TrySendMessage(unlockRequest, "unlock request"))
            {
                this.logger.AddDebugMessage("Unable to send unlock request.");
                return false;
            }

            for (int attempt = 1; attempt < MaxReceiveAttempts; attempt++)
            {
                Message unlockResponse = await this.device.ReceiveMessage();
                if (unlockResponse == null)
                {
                    this.logger.AddDebugMessage("No response to unlock request. Attempt #" + attempt.ToString());
                    continue;
                }

                string errorMessage;
                Response<bool> result = this.messageParser.ParseUnlockResponse(unlockResponse.GetBytes(), out errorMessage);
                if (errorMessage == null)
                {
                    return result.Value;
                }

                this.logger.AddUserMessage(errorMessage);
            }

            this.logger.AddUserMessage("Unable to process unlock response.");
            return false;
        }

        /// <summary>
        /// Try to send a message, retrying if necessary.
        /// </summary
        private async Task<bool> TrySendMessage(Message message, string description, int maxAttempts = MaxSendAttempts)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (await this.device.SendMessage(message))
                {
                    return true;
                }

                this.logger.AddDebugMessage("Unable to send " + description + " message. Attempt #" + attempt.ToString());
            }

            return false;
        }

        /// <summary>
        /// Wait for an incoming message.
        /// </summary>
        private async Task<Message> ReceiveMessage(CancellationToken cancellationToken)
        {
            Message response = null;

            for (int pause = 0; pause < 3; pause++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                response = await this.device.ReceiveMessage();
                if (response == null)
                {
                    this.logger.AddDebugMessage("No response to read request yet.");
                    await Task.Delay(10);
                    continue;
                }

                break;
            }

            return response;
        }

        /// <summary>
        /// Read messages from the device, ignoring irrelevant messages.
        /// </summary>
        private async Task<bool> WaitForSuccess(Func<Message, Response<bool>> filter, CancellationToken cancellationToken, int attempts = MaxReceiveAttempts)
        {
            for(int attempt = 1; attempt<=attempts; attempt++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                Message message = await this.device.ReceiveMessage();
                if(message == null)
                {
                    continue;
                }

                Response<bool> response = filter(message);
                if (response.Status != ResponseStatus.Success)
                {
                    this.logger.AddDebugMessage("Ignoring message: " + response.Status);
                    continue;
                }

                this.logger.AddDebugMessage("Found response, " + (response.Value ? "succeeded." : "failed."));
                return response.Value;
            }

            return false;
        }
    }
}
