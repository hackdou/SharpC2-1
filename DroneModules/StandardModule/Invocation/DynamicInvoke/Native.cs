using System;
using System.Runtime.InteropServices;

namespace StandardApi.Invocation.DynamicInvoke
{
    public static class Native
    {
        public static bool NtQueryInformationProcessWow64Information(IntPtr hProcess)
        {
            var result = NtQueryInformationProcess(hProcess, Data.Native.PROCESSINFOCLASS.ProcessWow64Information,
                out var pProcInfo);

            if (result != 0)
                throw new UnauthorizedAccessException("Access is denied.");

            return Marshal.ReadIntPtr(pProcInfo) != IntPtr.Zero;
        }

        public static IntPtr NtAllocateVirtualMemory(IntPtr processHandle, ref IntPtr baseAddress, IntPtr zeroBits,
            ref IntPtr regionSize, uint allocationType, uint protect)
        {
            object[] parameters =
            {
                processHandle, baseAddress, zeroBits, regionSize, allocationType, protect
            };

            _ = (uint)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll",
                "NtAllocateVirtualMemory", typeof(Delegates.NtAllocateVirtualMemory), ref parameters);

            baseAddress = (IntPtr)parameters[1];
            return baseAddress;
        }

        public static void NtFreeVirtualMemory(IntPtr processHandle, ref IntPtr baseAddress, ref IntPtr regionSize,
            uint freeType)
        {
            object[] parameters = { processHandle, baseAddress, regionSize, freeType };

            _ = (uint)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll",
                "NtFreeVirtualMemory", typeof(Delegates.NtFreeVirtualMemory), ref parameters);
        }

        public static uint NtCreateThreadEx(ref IntPtr threadHandle, Data.Win32.WinNT.ACCESS_MASK desiredAccess,
            IntPtr objectAttributes, IntPtr processHandle, IntPtr startAddress, IntPtr parameter, bool createSuspended,
            int stackZeroBits, int sizeOfStack, int maximumStackSize, IntPtr attributeList)
        {
            object[] parameters =
            {
                threadHandle, desiredAccess, objectAttributes, processHandle, startAddress, parameter, createSuspended,
                stackZeroBits,
                sizeOfStack, maximumStackSize, attributeList
            };

            var result = (uint)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll",
                "NtCreateThreadEx", typeof(Delegates.NtCreateThreadEx), ref parameters);

            threadHandle = (IntPtr)parameters[0];
            return result;
        }

        public static uint RtlCreateUserThread(IntPtr hProcess, IntPtr threadSecurityDescriptor, bool createSuspended,
            IntPtr zeroBits, IntPtr maximumStackSize, IntPtr committedStackSize, IntPtr startAddress,
            IntPtr parameter, ref IntPtr hThread, IntPtr clientId)
        {
            object[] parameters =
            {
                hProcess, threadSecurityDescriptor, createSuspended, zeroBits,
                maximumStackSize, committedStackSize, startAddress, parameter,
                hThread, clientId
            };

            var result = (uint)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll",
                "RtlCreateUserThread", typeof(Delegates.RtlCreateUserThread), ref parameters);

            hThread = (IntPtr)parameters[8];
            return result;
        }

        public static uint NtUnmapViewOfSection(IntPtr hProc, IntPtr baseAddress)
        {
            object[] parameters = { hProc, baseAddress };

            var result = (uint)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll",
                "NtUnmapViewOfSection", typeof(Delegates.NtUnmapViewOfSection), ref parameters);

            return result;
        }

        public static IntPtr NtOpenProcess(uint pid, Data.Win32.Kernel32.ProcessAccessFlags desiredAccess)
        {
            var hProcess = IntPtr.Zero;

            var oa = new Data.Native.OBJECT_ATTRIBUTES();
            var ci = new Data.Native.CLIENT_ID { UniqueProcess = (IntPtr)pid };

            object[] parameters = { hProcess, desiredAccess, oa, ci };

            _ = (uint)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll", "NtOpenProcess",
                typeof(Delegates.NtOpenProcess), ref parameters);

            hProcess = (IntPtr)parameters[0];
            return hProcess;
        }

        public static uint NtCreateSection(ref IntPtr sectionHandle, uint desiredAccess, IntPtr objectAttributes,
            ref ulong maximumSize, uint sectionPageProtection, uint allocationAttributes, IntPtr fileHandle)
        {
            object[] parameters =
            {
                sectionHandle, desiredAccess, objectAttributes, maximumSize, sectionPageProtection,
                allocationAttributes, fileHandle
            };

            var result = (uint)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll",
                "NtCreateSection", typeof(Delegates.NtCreateSection), ref parameters);

            sectionHandle = (IntPtr)parameters[0];
            maximumSize = (ulong)parameters[3];

            return result;
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

            var result = (uint)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll",
                "NtMapViewOfSection", typeof(Delegates.NtMapViewOfSection), ref parameters);
            
            baseAddress = (IntPtr)parameters[2];
            viewSize = (ulong)parameters[6];

            return result;
        }

        public static void RtlInitUnicodeString(ref Data.Native.UNICODE_STRING destinationString,
            [MarshalAs(UnmanagedType.LPWStr)] string sourceString)
        {

            object[] parameters = { destinationString, sourceString };

            Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll", "RtlInitUnicodeString",
                typeof(Delegates.RtlInitUnicodeString), ref parameters);

            destinationString = (Data.Native.UNICODE_STRING)parameters[0];
        }

        public static uint LdrLoadDll(IntPtr pathToFile, uint dwFlags, ref Data.Native.UNICODE_STRING moduleFileName,
            ref IntPtr moduleHandle)
        {
            object[] parameters = { pathToFile, dwFlags, moduleFileName, moduleHandle };

            var result = (uint)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll",
                "LdrLoadDll", typeof(Delegates.LdrLoadDll), ref parameters);

            moduleHandle = (IntPtr)parameters[3];
            return result;
        }

        public static void RtlZeroMemory(IntPtr destination, int length)
        {
            object[] parameters = { destination, length };
            
            Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll", "RtlZeroMemory",
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
                    var PBI = new Data.Native.PROCESS_BASIC_INFORMATION();
                    pProcInfo = Marshal.AllocHGlobal(Marshal.SizeOf(PBI));
                    RtlZeroMemory(pProcInfo, Marshal.SizeOf(PBI));
                    Marshal.StructureToPtr(PBI, pProcInfo, true);
                    processInformationLength = Marshal.SizeOf(PBI);
                    break;

                default:
                    throw new InvalidOperationException($"Invalid ProcessInfoClass: {processInfoClass}");
            }

            object[] parameters = { hProcess, processInfoClass, pProcInfo, processInformationLength, retLen };

            var result = (uint)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll",
                "NtQueryInformationProcess", typeof(Delegates.NtQueryInformationProcess), ref parameters);

            pProcInfo = (IntPtr)parameters[2];
            return result;
        }

        public static Data.Native.PROCESS_BASIC_INFORMATION NtQueryInformationProcessBasicInformation(IntPtr hProcess)
        {
            _ = NtQueryInformationProcess(hProcess, Data.Native.PROCESSINFOCLASS.ProcessBasicInformation,
                out var pProcInfo);
            
            return (Data.Native.PROCESS_BASIC_INFORMATION)Marshal.PtrToStructure(pProcInfo,
                typeof(Data.Native.PROCESS_BASIC_INFORMATION));
        }

        public static uint NtProtectVirtualMemory(IntPtr processHandle, ref IntPtr baseAddress, ref IntPtr regionSize,
            uint newProtect)
        {
            uint oldProtect = 0;

            object[] parameters = { processHandle, baseAddress, regionSize, newProtect, oldProtect };

            _ = (uint)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll",
                "NtProtectVirtualMemory", typeof(Delegates.NtProtectVirtualMemory), ref parameters);

            oldProtect = (uint)parameters[4];
            return oldProtect;
        }

        public static uint NtWriteVirtualMemory(IntPtr processHandle, IntPtr baseAddress, IntPtr buffer,
            uint bufferLength)
        {
            uint bytesWritten = 0;
            object[] parameters = { processHandle, baseAddress, buffer, bufferLength, bytesWritten };

            _ = (uint)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll",
                "NtWriteVirtualMemory", typeof(Delegates.NtWriteVirtualMemory), ref parameters);

            bytesWritten = (uint)parameters[4];
            return bytesWritten;
        }

        public static IntPtr LdrGetProcedureAddress(IntPtr hModule, IntPtr functionName, IntPtr ordinal,
            ref IntPtr functionAddress)
        {
            object[] parameters = { hModule, functionName, ordinal, functionAddress };

            _ = (uint)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll",
                "LdrGetProcedureAddress", typeof(Delegates.LdrGetProcedureAddress), ref parameters);

            functionAddress = (IntPtr)parameters[3];
            return functionAddress;
        }

        public static void RtlGetVersion(ref Data.Native.OSVERSIONINFOEX versionInformation)
        {
            object[] parameters = { versionInformation };

            _ = (uint)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll",
                "RtlGetVersion", typeof(Delegates.RtlGetVersion), ref parameters);
            
            versionInformation = (Data.Native.OSVERSIONINFOEX)parameters[0];
        }

        public static IntPtr NtOpenFile(ref IntPtr fileHandle, Data.Win32.Kernel32.FileAccessFlags desiredAccess,
            ref Data.Native.OBJECT_ATTRIBUTES objectAttributes, ref Data.Native.IO_STATUS_BLOCK ioStatusBlock,
            Data.Win32.Kernel32.FileShareFlags shareAccess, Data.Win32.Kernel32.FileOpenFlags openOptions)
        {
            object[] parameters =
            {
                fileHandle, desiredAccess, objectAttributes, ioStatusBlock, shareAccess, openOptions
            };

            _ = (uint)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("ntdll.dll",
                "NtOpenFile", typeof(Delegates.NtOpenFile), ref parameters);
            
            fileHandle = (IntPtr)parameters[0];
            return fileHandle;
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
            public delegate uint NtFreeVirtualMemory(
                IntPtr processHandle,
                ref IntPtr baseAddress,
                ref IntPtr regionSize,
                uint freeType);
            
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
            public delegate uint NtOpenProcess(
                ref IntPtr processHandle,
                Data.Win32.Kernel32.ProcessAccessFlags desiredAccess,
                ref Data.Native.OBJECT_ATTRIBUTES objectAttributes,
                ref Data.Native.CLIENT_ID clientId);

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
                ref IntPtr baseAddress,
                IntPtr zeroBits,
                IntPtr commitSize,
                IntPtr sectionOffset,
                ref ulong viewSize,
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

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint NtProtectVirtualMemory(
                IntPtr processHandle,
                ref IntPtr baseAddress,
                ref IntPtr regionSize,
                uint newProtect,
                ref uint oldProtect);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint NtWriteVirtualMemory(
                IntPtr processHandle,
                IntPtr baseAddress,
                IntPtr buffer,
                uint bufferLength,
                ref uint bytesWritten);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint LdrGetProcedureAddress(
                IntPtr hModule,
                IntPtr functionName,
                IntPtr ordinal,
                ref IntPtr functionAddress);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint RtlGetVersion(
                ref Data.Native.OSVERSIONINFOEX versionInformation);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint NtOpenFile(
                ref IntPtr fileHandle,
                Data.Win32.Kernel32.FileAccessFlags desiredAccess,
                ref Data.Native.OBJECT_ATTRIBUTES objectAttributes,
                ref Data.Native.IO_STATUS_BLOCK ioStatusBlock,
                Data.Win32.Kernel32.FileShareFlags shareAccess,
                Data.Win32.Kernel32.FileOpenFlags openOptions);
        }
    }
}