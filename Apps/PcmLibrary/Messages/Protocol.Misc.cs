using System;
using System.Collections.Generic;
using System.Text;

namespace PcmHacking
{
    public partial class Protocol
    {
        /// <summary>
        /// Tell the bus that a test device is present.
        /// </summary>
        /// <remarks>
        /// Changing the priority byte to Physical0 (rather than Physical0High) 
        /// broke PCM Hammer's flash operations. 
        /// </remarks>
        public Message CreateTestDevicePresentNotification()
        {
            byte[] bytes = new byte[] { Priority.Physical0High, DeviceId.Broadcast, DeviceId.Tool, Mode.TestDevicePresent };
            return new Message(bytes);
        }

        /// <summary>
        /// Tell the bus that a test device is present.
        /// </summary>
        /// <remarks>
        /// Using Physical0 priority keeps subsequent data log messages using 
        /// Physical0. Using Physical0High caused the PCM to switch to 
        /// Physical0High for subsequent data log messages. Using Physical0
        /// simplifies the code that receives data logging messages.
        /// </remarks>
        public Message CreateDataLoggerPresentNotification()
        {
            byte[] bytes = new byte[] { Priority.Physical0, DeviceId.Broadcast, DeviceId.Tool, Mode.TestDevicePresent };
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
            byte[] Bytes = new byte[] { Priority.Physical0, DeviceId.Broadcast, DeviceId.Tool, Mode.SilenceBus, Submode.Null };
            return new Message(Bytes);
        }

        /// <summary>
        /// Create a broadcast message telling all devices to disable normal message transmission (disable chatter)
        /// </summary>
        public Message CreateDisableNormalMessageTransmissionOK()
        {
            byte[] bytes = new byte[] { Priority.Physical0, DeviceId.Tool, DeviceId.Pcm, Mode.SilenceBus + Mode.Response, Submode.Null };
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

        public Response<bool> ParseRecoveryModeBroadcast(Message message)
        {
            Response<bool> rc = this.DoSimpleValidation(message, 0x6C, 0x62, 0x01);
            if (!rc.Value)
            {
                rc = this.DoSimpleValidation(message, 0x6C, 0x62, 0x00);
            }

            return rc;
        }        
    }
}
