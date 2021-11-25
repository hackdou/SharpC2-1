// Author: Ryan Cobb (@cobbr_io), The Wover (@TheRealWover)
// Project: SharpSploit (https://github.com/cobbr/SharpSploit)
// License: BSD 3-Clause

using System;
using System.Runtime.InteropServices;

namespace DroneService.Invocation.DynamicInvoke
{
    public static class Native
    {
        public static IntPtr NtAllocateVirtualMemory(IntPtr processHandle, ref IntPtr baseAddress, IntPtr zeroBits,
            ref IntPtr regionSize, uint allocationType, uint protect)
        {
            object[] parameters = { processHandle, baseAddress, zeroBits, regionSize, allocationType, protect };

            _ = (uint)Generic.DynamicAPIInvoke("ntdll.dll", "NtAllocateVirtualMemory",
                typeof(Delegates.NtAllocateVirtualMemory), ref parameters);

            baseAddress = (IntPtr)parameters[1];
            return baseAddress;
        }

        public static uint NtWriteVirtualMemory(IntPtr processHandle, IntPtr baseAddress, IntPtr buffer,
            uint bufferLength)
        {
            uint bytesWritten = 0;
            object[] parameters = { processHandle, baseAddress, buffer, bufferLength, bytesWritten };

            _ = (uint)Generic.DynamicAPIInvoke("ntdll.dll", "NtWriteVirtualMemory",
                typeof(Delegates.NtWriteVirtualMemory), ref parameters);

            bytesWritten = (uint)parameters[4];
            return bytesWritten;
        }

        public static uint NtProtectVirtualMemory(IntPtr processHandle, ref IntPtr baseAddress, ref IntPtr regionSize,
            uint newProtect)
        {
            uint oldProtect = 0;
            object[] parameters = { processHandle, baseAddress, regionSize, newProtect, oldProtect};

            _ = (uint)Generic.DynamicAPIInvoke("ntdll.dll", "NtProtectVirtualMemory",
                typeof(Delegates.NtProtectVirtualMemory), ref parameters);

            oldProtect = (uint)parameters[4];
            return oldProtect;
        }

        public static uint NtCreateThreadEx(ref IntPtr threadHandle, Data.Win32.WinNT.ACCESS_MASK desiredAccess,
            IntPtr objectAttributes, IntPtr processHandle, IntPtr startAddress, IntPtr parameter, bool createSuspended,
            int stackZeroBits, int sizeOfStack, int maximumStackSize, IntPtr attributeList)
        {
            object[] parameters =
            {
                threadHandle, desiredAccess, objectAttributes, processHandle, startAddress, parameter, createSuspended, stackZeroBits,
                sizeOfStack, maximumStackSize, attributeList
            };

            var retValue = (uint)Generic.DynamicAPIInvoke("ntdll.dll", "NtCreateThreadEx",
                typeof(Delegates.NtCreateThreadEx), ref parameters);

            threadHandle = (IntPtr)parameters[0];
            return retValue;
        }

        public static uint RtlCreateUserThread(IntPtr process, IntPtr threadSecurityDescriptor,
            bool createSuspended, IntPtr zeroBits, IntPtr maximumStackSize, IntPtr committedStackSize,
            IntPtr startAddress, IntPtr parameter, ref IntPtr thread, IntPtr clientId)
        {
            object[] parameters =
            {
                process, threadSecurityDescriptor, createSuspended, zeroBits,
                maximumStackSize, committedStackSize, startAddress, parameter,
                thread, clientId
            };

            var retValue = (uint)Generic.DynamicAPIInvoke("ntdll.dll", "RtlCreateUserThread",
                typeof(Delegates.RtlCreateUserThread), ref parameters);

            thread = (IntPtr)parameters[8];
            return retValue;
        }

        public static uint NtUnmapViewOfSection(IntPtr hProc, IntPtr baseAddress)
        {
            object[] parameters = { hProc, baseAddress };

            var result = (uint)Generic.DynamicAPIInvoke("ntdll.dll", "NtUnmapViewOfSection",
                typeof(Delegates.NtUnmapViewOfSection), ref parameters);

            return result;
        }

        public static uint NtCreateSection(ref IntPtr sectionHandle, uint desiredAccess, IntPtr objectAttributes,
            ref ulong maximumSize, uint sectionPageProtection, uint allocationAttributes, IntPtr fileHandle)
        {

            object[] parameters =
            {
                sectionHandle, desiredAccess, objectAttributes, maximumSize, sectionPageProtection,
                allocationAttributes, fileHandle
            };

            var retValue = (uint)Generic.DynamicAPIInvoke("ntdll.dll", "NtCreateSection",
                typeof(Delegates.NtCreateSection), ref parameters);

            sectionHandle = (IntPtr)parameters[0];
            maximumSize = (ulong)parameters[3];

            return retValue;
        }

        public static uint NtMapViewOfSection(IntPtr sectionHandle, IntPtr processHandle, ref IntPtr baseAddress,
            IntPtr zeroBits, IntPtr commitSize, IntPtr sectionOffset, ref ulong viewSize, uint inheritDisposition,
            uint allocationType, uint win32Protect)
        {
            object[] parameters =
            {
                sectionHandle, processHandle, baseAddress, zeroBits, commitSize, sectionOffset, viewSize,
                inheritDisposition, allocationType,
                win32Protect
            };

            var retValue = (uint)Generic.DynamicAPIInvoke("ntdll.dll", "NtMapViewOfSection",
                typeof(Delegates.NtMapViewOfSection), ref parameters);


            baseAddress = (IntPtr)parameters[2];
            viewSize = (ulong)parameters[6];

            return retValue;
        }

        public static void RtlInitUnicodeString(ref Data.Native.UNICODE_STRING destinationString,
            [MarshalAs(UnmanagedType.LPWStr)] string sourceString)
        {
            object[] parameters = { destinationString, sourceString };

            Generic.DynamicAPIInvoke("ntdll.dll", "RtlInitUnicodeString",
                typeof(Delegates.RtlInitUnicodeString), ref parameters);

            destinationString = (Data.Native.UNICODE_STRING)parameters[0];
        }

        public static uint LdrLoadDll(IntPtr pathToFile, uint dwFlags, ref Data.Native.UNICODE_STRING moduleFileName,
            ref IntPtr moduleHandle)
        {
            object[] parameters = { pathToFile, dwFlags, moduleFileName, moduleHandle };

            var retValue = (uint)Generic.DynamicAPIInvoke("ntdll.dll", "LdrLoadDll",
                typeof(Delegates.LdrLoadDll), ref parameters);

            moduleHandle = (IntPtr)parameters[3];
            return retValue;
        }

        public static void RtlZeroMemory(IntPtr destination, int length)
        {
            object[] parameters = { destination, length };

            Generic.DynamicAPIInvoke("ntdll.dll", "RtlZeroMemory",
                typeof(Delegates.RtlZeroMemory), ref parameters);
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

            var retValue = (uint)Generic.DynamicAPIInvoke("ntdll.dll", "NtQueryInformationProcess",
                typeof(Delegates.NtQueryInformationProcess), ref parameters);

            pProcInfo = (IntPtr)parameters[2];
            return retValue;
        }

        public static Data.Native.PROCESS_BASIC_INFORMATION NtQueryInformationProcessBasicInformation(IntPtr hProcess)
        {
            _ = NtQueryInformationProcess(hProcess, Data.Native.PROCESSINFOCLASS.ProcessBasicInformation,
                out var pProcInfo);

            return (Data.Native.PROCESS_BASIC_INFORMATION)Marshal.PtrToStructure(pProcInfo,
                typeof(Data.Native.PROCESS_BASIC_INFORMATION));
        }

        private struct Delegates
        {
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint NtAllocateVirtualMemory(
                IntPtr processHandle,
                ref IntPtr baseAddress,
                IntPtr zeroBits,
                ref IntPtr regionSize,
                uint allocationType,
                uint protect);
            
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint NtWriteVirtualMemory(
                IntPtr processHandle,
                IntPtr baseAddress,
                IntPtr buffer,
                uint bufferLength,
                ref uint bytesWritten);
            
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint NtProtectVirtualMemory(
                IntPtr processHandle,
                ref IntPtr baseAddress,
                ref IntPtr regionSize,
                uint newProtect,
                ref uint oldProtect);
            
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint NtCreateThreadEx(
                out IntPtr threadHandle,
                Data.Win32.WinNT.ACCESS_MASK desiredAccess,
                IntPtr objectAttributes,
                IntPtr processHandle,
                IntPtr startAddress,
                IntPtr parameter,
                bool createSuspended,
                int stackZeroBits,
                int sizeOfStack,
                int maximumStackSize,
                IntPtr attributeList);
            
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint RtlCreateUserThread(
                IntPtr process,
                IntPtr threadSecurityDescriptor,
                bool createSuspended,
                IntPtr zeroBits,
                IntPtr maximumStackSize,
                IntPtr committedStackSize,
                IntPtr startAddress,
                IntPtr parameter,
                ref IntPtr thread,
                IntPtr clientId);
            
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint NtCreateSection(
                ref IntPtr sectionHandle,
                uint desiredAccess,
                IntPtr objectAttributes,
                ref ulong maximumSize,
                uint sectionPageProtection,
                uint allocationAttributes,
                IntPtr fileHandle);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint NtMapViewOfSection(
                IntPtr sectionHandle,
                IntPtr processHandle,
                out IntPtr baseAddress,
                IntPtr zeroBits,
                IntPtr commitSize,
                IntPtr sectionOffset,
                out ulong viewSize,
                uint inheritDisposition,
                uint allocationType,
                uint win32Protect);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint LdrLoadDll(
                IntPtr pathToFile,
                uint dwFlags,
                ref Data.Native.UNICODE_STRING moduleFileName,
                ref IntPtr moduleHandle);
            
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void RtlInitUnicodeString(
                ref Data.Native.UNICODE_STRING destinationString,
                [MarshalAs(UnmanagedType.LPWStr)]
                string sourceString);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void RtlZeroMemory(
                IntPtr destination,
                int length);
            
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint NtUnmapViewOfSection(
                IntPtr hProc,
                IntPtr baseAddress);

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