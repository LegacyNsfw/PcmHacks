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
using System;
using System.Runtime.InteropServices;

namespace J2534DotNet
{
    internal unsafe struct UnsafePassThruMsg
    {
        public int ProtocolID;
        public int RxStatus;
        public int TxFlags;
        public int Timestamp;
        public int DataSize;
        public int ExtraDataIndex;
        public fixed byte Data[4128];
    }

    internal static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
    }

    internal class J2534DllWrapper
    {
        private IntPtr m_pDll;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int PassThruOpen(IntPtr pDeviceName, IntPtr deviceId);
        public PassThruOpen Open;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int PassThruClose(int deviceId);
        public PassThruClose Close;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int PassThruConnect(int deviceId, int protocolId, int flags, int baudRate, ref int channelId);
        public PassThruConnect Connect;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int PassThruDisconnect(int channelId);
        public PassThruDisconnect Disconnect;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int PassThruReadMsgs(int channelId, IntPtr pMsgs, ref int numMsgs, int timeout);
        public PassThruReadMsgs ReadMsgs;
        //extern “C” long WINAPI PassThruReadMsgs
        //(
        //unsigned long ChannelID,
        //PASSTHRU_MSG *pMsg,
        //unsigned long *pNumMsgs,
        //unsigned long Timeout
        //)

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int PassThruWriteMsgs(int channelId, ref UnsafePassThruMsg msg, ref int numMsgs, int timeout);
        public PassThruWriteMsgs WriteMsgs;
        //extern “C” long WINAPI PassThruWriteMsgs
        //(
        //unsigned long ChannelID,
        //PASSTHRU_MSG *pMsg,
        //unsigned long *pNumMsgs,
        //unsigned long Timeout
        //)

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int PassThruStartPeriodicMsg(int channelId, ref UnsafePassThruMsg msg, ref int msgId, int timeInterval);
        public PassThruStartPeriodicMsg StartPeriodicMsg;
        //extern “C” long WINAPI PassThruStartPeriodicMsg
        //(
        //unsigned long ChannelID,
        //PASSTHRU_MSG *pMsg,
        //unsigned long *pMsgID,
        //unsigned long TimeInterval
        //)

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int PassThruStopPeriodicMsg(int channelId, int msgId);
        public PassThruStopPeriodicMsg StopPeriodicMsg;
        //extern “C” long WINAPI PassThruStopPeriodicMsg
        //(
        //unsigned long ChannelID,
        //unsigned long MsgID
        //)

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int PassThruStartMsgFilter
        (
            int channelid,
            int filterType,
            ref UnsafePassThruMsg maskMsg,
            ref UnsafePassThruMsg patternMsg,
            ref UnsafePassThruMsg flowControlMsg,
            ref int filterId
        );
        public PassThruStartMsgFilter StartMsgFilter;
        //extern “C” long WINAPI PassThruStartMsgFilter
        //(
        //unsigned long ChannelID,
        //unsigned long FilterType,
        //PASSTHRU_MSG *pMaskMsg,
        //PASSTHRU_MSG *pPatternMsg,
        //PASSTHRU_MSG *pFlowControlMsg,
        //unsigned long *pFilterID
        //)

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int PassThruStartPassBlockMsgFilter
        (
            int channelid,
            int filterType,
            ref UnsafePassThruMsg uMaskMsg,
            ref UnsafePassThruMsg uPatternMsg,
            int nada,
            ref int filterId
        );
        public PassThruStartPassBlockMsgFilter StartPassBlockMsgFilter;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int PassThruStopMsgFilter(int channelId, int filterId);
        public PassThruStopMsgFilter StopMsgFilter;
        //extern “C” long WINAPI PassThruStopMsgFilter
        //(
        //unsigned long ChannelID,
        //unsigned long FilterID
        //)

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int PassThruSetProgrammingVoltage(int deviceId, int pinNumber, int voltage);
        public PassThruSetProgrammingVoltage SetProgrammingVoltage;
        //extern “C” long WINAPI PassThruSetProgrammingVoltage
        //(
        //unsigned long DeviceID,
        //unsigned long PinNumber,
        //unsigned long Voltage
        //)        

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int PassThruReadVersion(int deviceId, IntPtr firmwareVersion, IntPtr dllVersion, IntPtr apiVersion);
        public PassThruReadVersion ReadVersion;
        //extern “C” long WINAPI PassThruReadVersion
        //(
        //unsigned long DeviceID
        //char *pFirmwareVersion,
        //char *pDllVersion,
        //char *pApiVersion
        //)

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int PassThruGetLastError(IntPtr errorDescription);
        public PassThruGetLastError GetLastError;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int PassThruIoctl(int channelId, int ioctlID, IntPtr input, IntPtr output);
        public PassThruIoctl Ioctl;
        //extern “C” long WINAPI PassThruIoctl
        //(
        //unsigned long ChannelID,
        //unsigned long IoctlID,
        //void *pInput,
        //void *pOutput
        //)

        public bool LoadJ2534Library(string path)
        {
            m_pDll = NativeMethods.LoadLibrary(path);

            if (m_pDll == IntPtr.Zero)
                return false;

            IntPtr pAddressOfFunctionToCall = NativeMethods.GetProcAddress(m_pDll, "PassThruOpen");
            if (pAddressOfFunctionToCall != IntPtr.Zero)
                Open = (PassThruOpen)Marshal.GetDelegateForFunctionPointer(
                                                                                    pAddressOfFunctionToCall,
                                                                                    typeof(PassThruOpen));

            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(m_pDll, "PassThruClose");
            if (pAddressOfFunctionToCall != IntPtr.Zero)
                Close = (PassThruClose)Marshal.GetDelegateForFunctionPointer(
                                                                                    pAddressOfFunctionToCall,
                                                                                    typeof(PassThruClose));

            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(m_pDll, "PassThruConnect");
            if (pAddressOfFunctionToCall != IntPtr.Zero)
                Connect = (PassThruConnect)Marshal.GetDelegateForFunctionPointer(
                                                                                        pAddressOfFunctionToCall,
                                                                                        typeof(PassThruConnect));

            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(m_pDll, "PassThruDisconnect");
            if (pAddressOfFunctionToCall != IntPtr.Zero)
                Disconnect = (PassThruDisconnect)Marshal.GetDelegateForFunctionPointer(
                                                                                        pAddressOfFunctionToCall,
                                                                                        typeof(PassThruDisconnect));

            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(m_pDll, "PassThruReadMsgs");
            if (pAddressOfFunctionToCall != IntPtr.Zero)
                ReadMsgs = (PassThruReadMsgs)Marshal.GetDelegateForFunctionPointer(
                                                                                        pAddressOfFunctionToCall,
                                                                                        typeof(PassThruReadMsgs));

            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(m_pDll, "PassThruWriteMsgs");
            if (pAddressOfFunctionToCall != IntPtr.Zero)
                WriteMsgs = (PassThruWriteMsgs)Marshal.GetDelegateForFunctionPointer(
                                                                                        pAddressOfFunctionToCall,
                                                                                        typeof(PassThruWriteMsgs));

            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(m_pDll, "PassThruStartPeriodicMsg");
            if (pAddressOfFunctionToCall != IntPtr.Zero)
                StartPeriodicMsg = (PassThruStartPeriodicMsg)Marshal.GetDelegateForFunctionPointer(
                                                                                        pAddressOfFunctionToCall,
                                                                                        typeof(PassThruStartPeriodicMsg));

            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(m_pDll, "PassThruStopPeriodicMsg");
            if (pAddressOfFunctionToCall != IntPtr.Zero)
                StopPeriodicMsg = (PassThruStopPeriodicMsg)Marshal.GetDelegateForFunctionPointer(
                                                                                        pAddressOfFunctionToCall,
                                                                                        typeof(PassThruStopPeriodicMsg));

            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(m_pDll, "PassThruStartMsgFilter");
            if (pAddressOfFunctionToCall != IntPtr.Zero)
                StartMsgFilter = (PassThruStartMsgFilter)Marshal.GetDelegateForFunctionPointer(
                                                                                        pAddressOfFunctionToCall,
                                                                                        typeof(PassThruStartMsgFilter));

#warning This address may not be correct
            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(m_pDll, "PassThruStartMsgFilter");
            if (pAddressOfFunctionToCall != IntPtr.Zero)
                StartPassBlockMsgFilter = (PassThruStartPassBlockMsgFilter)Marshal.GetDelegateForFunctionPointer(
                                                                                        pAddressOfFunctionToCall,
                                                                                        typeof(PassThruStartPassBlockMsgFilter));

            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(m_pDll, "PassThruStopMsgFilter");
            if (pAddressOfFunctionToCall != IntPtr.Zero)
                StopMsgFilter = (PassThruStopMsgFilter)Marshal.GetDelegateForFunctionPointer(
                                                                                        pAddressOfFunctionToCall,
                                                                                        typeof(PassThruStopMsgFilter));

            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(m_pDll, "PassThruSetProgrammingVoltage");
            if (pAddressOfFunctionToCall != IntPtr.Zero)
                SetProgrammingVoltage = (PassThruSetProgrammingVoltage)Marshal.GetDelegateForFunctionPointer(
                                                                                        pAddressOfFunctionToCall,
                                                                                        typeof(PassThruSetProgrammingVoltage));

            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(m_pDll, "PassThruReadVersion");
            if (pAddressOfFunctionToCall != IntPtr.Zero)
                ReadVersion = (PassThruReadVersion)Marshal.GetDelegateForFunctionPointer(
                                                                                        pAddressOfFunctionToCall,
                                                                                        typeof(PassThruReadVersion));

            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(m_pDll, "PassThruGetLastError");
            if (pAddressOfFunctionToCall != IntPtr.Zero)
                GetLastError = (PassThruGetLastError)Marshal.GetDelegateForFunctionPointer(
                                                                                        pAddressOfFunctionToCall,
                                                                                        typeof(PassThruGetLastError));

            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(m_pDll, "PassThruIoctl");
            if (pAddressOfFunctionToCall != IntPtr.Zero)
                Ioctl = (PassThruIoctl)Marshal.GetDelegateForFunctionPointer(
                                                                                        pAddressOfFunctionToCall,
                                                                                        typeof(PassThruIoctl));

            return true;
        }

        public bool FreeLibrary()
        {
            return NativeMethods.FreeLibrary(m_pDll);
        }
    }
}
