using System;
using System.Collections.Generic;
using System.Linq;

namespace PcmHacking
{
    public partial class Protocol
    {
        /// <summary>
        /// Tell the bus that a test device is present.
        /// </summary>
        public Message CreateTestDevicePresentNotification()
        {
            byte[] bytes = new byte[] { Priority.Physical0High, DeviceId.Broadcast, DeviceId.Tool, Mode.TestDevicePresent };
            return new Message(bytes);
        }

        /// <summary>
        /// Create a broadcast message telling all modules to clear diagnostic trouble codes.
        /// </summary>
        public Message CreateClearDiagnosticTroubleCodesRequest()
        {
            byte[] bytes = new byte[] { Priority.Functional0, 0x6A, DeviceId.Tool, Mode.ClearDiagnosticTroubleCodes };
            return new Message(bytes);
        }

        /// <summary>
        /// Create a broadcast message telling all modules to clear diagnostic information.
        /// </summary>
        public Message CreateClearDiagnosticInformationRequest()
        {
            byte[] bytes = new byte[] { Priority.Physical0High, DeviceId.Broadcast, DeviceId.Tool, Mode.ClearDiagnosticInformation };
            return new Message(bytes);
        }

        /// <summary>
        /// Create a broadcast message telling all devices to disable normal message transmission (disable chatter)
        /// </summary>
        public Message CreateDisableNormalMessageTransmission()
        {
            byte[] Bytes = new byte[] { Priority.Physical0, DeviceId.Broadcast, DeviceId.Tool, Mode.SilenceBus, SubMode.Null };
            return new Message(Bytes);
        }

        /// <summary>
        /// Create a broadcast message telling all devices to disable normal message transmission (disable chatter)
        /// </summary>
        public Message CreateDisableNormalMessageTransmissionOK()
        {
            byte[] bytes = new byte[] { Priority.Physical0, DeviceId.Tool, DeviceId.Pcm, Mode.SilenceBus + Mode.Response, SubMode.Null };
            return new Message(bytes);
        }

        /// <summary>
        /// Create a broadcast message telling all devices to clear their DTCs
        /// </summary>
        public Message ClearDTCs()
        {
            byte[] bytes = new byte[] { Priority.Functional0, 0x6A, DeviceId.Tool, Mode.ClearDiagnosticTroubleCodes };
            return new Message(bytes);
        }

        /// <summary>
        /// PCM Response to Clear DTCs
        /// </summary>
        public Message ClearDTCsOK()
        {
            byte[] bytes = new byte[] { Priority.Functional0Low, 0x6B, DeviceId.Pcm, Mode.ClearDiagnosticTroubleCodes + Mode.Response };
            return new Message(bytes);
        }

        /// <summary>
        /// Check for the messages sent by the PCM when it boots into recovery mode.
        /// </summary>
        public Response<bool> ParseRecoveryModeBroadcast(Message message)
        {
            Response<bool> rc = this.DoSimpleValidation(message, 0x6C, 0x62, 0x01);
            if (!rc.Value) rc = this.DoSimpleValidation(message, 0x6C, 0xA2, 0x00);
            return rc;
        }

        /// <summary>
        /// The fourth byte of a display request needs to alternate between 0xB2 (1st, 3rd, 5th...) and 0x32 (2nd, 4th, 6th...).
        /// </summary>
        private bool oddDisplayRequest;

        /// <summary>
        /// This message must be sent before sending the actual content.
        /// </summary>
        public Message CreateBeginDisplayRequest()
        {
            this.oddDisplayRequest = true;
            return new Message(new byte[] { 0x8A, 0xEA, 0x10, 0xB1, 0x01, 0x10, 0x11, 0x14, 0x02, 0x00 });
        }

        /// <summary>
        /// Create a message to append text to the Driver Information Center (DIC) display.
        /// </summary>
        /// <remarks>
        /// The final bit of text must be 4 characters.
        /// All others must by 5 characters.
        /// The caller must pad with spaces in order to meet these requirements.
        /// </remarks>
        public Message CreateDisplayRequest(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException();
            }

            if (!((text.Length == 4) || (text.Length == 5)))
            {
                throw new ArgumentException("Text must be at 4 or 5 characters.");
            }

            byte alternate = this.oddDisplayRequest ? (byte)0xB2 : (byte)0x32;
            this.oddDisplayRequest = !this.oddDisplayRequest;

            byte[] ascii = System.Text.Encoding.ASCII.GetBytes(text);

            byte[] header = new byte[] { 0x8A, 0xEB, 0x10, alternate, 0x01, 0x11 };
            IEnumerable<byte> payload = ascii.Length == 5 ? ascii : ascii.Concat(new byte[] { 0x04 });
            return new Message(header.Concat(payload).ToArray());
        }
    }
}
