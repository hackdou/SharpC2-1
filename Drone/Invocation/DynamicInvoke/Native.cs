using System;
using System.Runtime.InteropServices;

namespace Drone.Invocation.DynamicInvoke
{
    public static class Native
    {
        public static void RtlInitUnicodeString(ref Data.Native.UNICODE_STRING destinationString,
            [MarshalAs(UnmanagedType.LPWStr)] string sourceString)
        {
            object[] parameters = { destinationString, sourceString };

            Generic.DynamicApiInvoke("ntdll.dll", "RtlInitUnicodeString",
                typeof(Delegates.RtlInitUnicodeString), ref parameters);

            destinationString = (Data.Native.UNICODE_STRING)parameters[0];
        }
        
        public static void RtlZeroMemory(IntPtr destination, int length)
        {
            object[] parameters = { destination, length };
            Generic.DynamicApiInvoke(@"ntdll.dll", @"RtlZeroMemory", typeof(Delegates.RtlZeroMemory), ref parameters);
        }

        public static uint LdrLoadDll(IntPtr pathToFile, uint dwFlags, ref Data.Native.UNICODE_STRING moduleFileName,
            ref IntPtr moduleHandle)
        {
            object[] parameters = { pathToFile, dwFlags, moduleFileName, moduleHandle };

            var retValue = (uint)Generic.DynamicApiInvoke("ntdll.dll", "LdrLoadDll",
                typeof(Delegates.LdrLoadDll), ref parameters);

            moduleHandle = (IntPtr)parameters[3];
            return retValue;
        }

        public static Data.Native.PROCESS_BASIC_INFORMATION NtQueryInformationProcessBasicInformation(IntPtr hProcess)
        {
            var retValue = NtQueryInformationProcess(
                hProcess,
                Data.Native.PROCESSINFOCLASS.ProcessBasicInformation,
                out var pProcInfo);
            
            if (retValue != 0)
                throw new UnauthorizedAccessException("Access is denied.");

            return (Data.Native.PROCESS_BASIC_INFORMATION)Marshal.PtrToStructure(
                pProcInfo,
                typeof(Data.Native.PROCESS_BASIC_INFORMATION));
        }

        public static uint NtQueryInformationProcess(IntPtr hProcess, Data.Native.PROCESSINFOCLASS processInfoClass,
            out IntPtr pProcInfo)
        {
            int processInformationLength;
            uint retLen = 0;

            switch (processInfoClass)
            {
                case Data.Native.PROCESSINFOCLASS.ProcessWow64Information:
                    pProcInfo = Marshal.AllocHGlobal(IntPtr.Size);
                    RtlZeroMemory(pProcInfo, IntPtr.Size);
                    processInformationLength = IntPtr.Size;
                    break;
                
                case Data.Native.PROCESSINFOCLASS.ProcessBasicInformation:
                    var pbi = new Data.Native.PROCESS_BASIC_INFORMATION();
                    pProcInfo = Marshal.AllocHGlobal(Marshal.SizeOf(pbi));
                    RtlZeroMemory(pProcInfo, Marshal.SizeOf(pbi));
                    Marshal.StructureToPtr(pbi, pProcInfo, true);
                    processInformationLength = Marshal.SizeOf(pbi);
                    break;
                
                default:
                    throw new InvalidOperationException($"Invalid ProcessInfoClass: {processInfoClass}");
            }

            object[] parameters = { hProcess, processInfoClass, pProcInfo, processInformationLength, retLen };

            var result = (uint)Generic.DynamicApiInvoke(@"ntdll.dll", @"NtQueryInformationProcess",
                typeof(Delegates.NtQueryInformationProcess), ref parameters);

            if (result != 0)
                throw new UnauthorizedAccessException("Access is denied.");

            pProcInfo = (IntPtr)parameters[2];
            return result;
        }

        private struct Delegates
        {
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void RtlZeroMemory(
                IntPtr destination,
                int length);
            
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void RtlInitUnicodeString(
                ref Data.Native.UNICODE_STRING destinationString,
                [MarshalAs(UnmanagedType.LPWStr)]
                string sourceString);
            
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint LdrLoadDll(
                IntPtr pathToFile,
                uint dwFlags,
                ref Data.Native.UNICODE_STRING moduleFileName,
                ref IntPtr moduleHandle);
            
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint NtQueryInformationProcess(
                IntPtr processHandle,
                Data.Native.PROCESSINFOCLASS processInformationClass,
                IntPtr processInformation,
                int processInformationLength,
                ref uint returnLength);
        }
    }
}