#region Copyright (c) 2010, Michael Kelly
/* 
 * Copyright (c) 2010, Michael Kelly
 * michael.e.kelly@gmail.com
 * http://michael-kelly.com/
 * 
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 * Neither the name of the organization nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 */
#endregion License
using System.Collections.Generic;

namespace J2534DotNet
{
    public interface IJ2534
    {
        bool LoadLibrary(J2534Device device);
        bool FreeLibrary();
        J2534Err Open(ref int deviceId);
        J2534Err Close(int deviceId);
        J2534Err Connect(int deviceId, ProtocolID protocolId, ConnectFlag flags, BaudRate baudRate, ref int channelId);
        J2534Err Connect(int deviceId, ProtocolID protocolId, ConnectFlag flags, int baudRate, ref int channelId);
        J2534Err Disconnect(int channelId);
        J2534Err ReadMsgs(int channelId, ref List<PassThruMsg> msgs, ref int numMsgs, int timeout);
        J2534Err WriteMsgs(int channelId, ref PassThruMsg msg, ref int numMsgs, int timeout);
        J2534Err StartPeriodicMsg(int channelId, ref PassThruMsg msg, ref int msgId, int timeInterval);
        J2534Err StopPeriodicMsg(int channelId, int msgId);
        J2534Err StartMsgFilter
        (
            int channelid,
            FilterType filterType,
            ref PassThruMsg maskMsg,
            ref PassThruMsg patternMsg,
            ref PassThruMsg flowControlMsg,
            ref int filterId
        );
        J2534Err StopMsgFilter(int channelId, int filterId);
        J2534Err SetProgrammingVoltage(int deviceId, PinNumber pinNumber, int voltage);
        J2534Err ReadVersion(int deviceId, ref string firmwareVersion, ref string dllVersion, ref string apiVersion);
        J2534Err GetLastError(ref string errorDescription);
        J2534Err GetConfig(int channelId, ref List<SConfig> config);
        J2534Err SetConfig(int channelId, ref List<SConfig> config);
        J2534Err ReadBatteryVoltage(int deviceId, ref int voltage);
        J2534Err FiveBaudInit(int channelId, byte targetAddress, ref byte keyword1, ref byte keyword2);
        J2534Err FastInit(int channelId, PassThruMsg txMsg, ref PassThruMsg rxMsg);
        J2534Err ClearTxBuffer(int channelId);
        J2534Err ClearRxBuffer(int channelId);
        J2534Err ClearPeriodicMsgs(int channelId);
        J2534Err ClearMsgFilters(int channelId);
        J2534Err ClearFunctMsgLookupTable(int channelId);
        J2534Err AddToFunctMsgLookupTable(int channelId);
        J2534Err DeleteFromFunctMsgLookupTable(int channelId);
    }
}
