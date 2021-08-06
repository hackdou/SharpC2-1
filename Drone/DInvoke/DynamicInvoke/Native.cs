// Author: Ryan Cobb (@cobbr_io), The Wover (@TheRealWover)
// Project: SharpSploit (https://github.com/cobbr/SharpSploit)
// License: BSD 3-Clause

using System;
using System.Runtime.InteropServices;


namespace Drone.DInvoke.DynamicInvoke
{
    public static class Native
    {
        public static Data.Native.NTSTATUS NtCreateSection(
            ref IntPtr SectionHandle,
            uint DesiredAccess,
            IntPtr ObjectAttributes,
            ref ulong MaximumSize,
            uint SectionPageProtection,
            uint AllocationAttributes,
            IntPtr FileHandle)
        {

            // Craft an array for the arguments
            object[] funcargs =
            {
                SectionHandle, DesiredAccess, ObjectAttributes, MaximumSize, SectionPageProtection, AllocationAttributes, FileHandle
            };

            Data.Native.NTSTATUS retValue = (Data.Native.NTSTATUS)Generic.DynamicAPIInvoke(@"ntdll.dll", @"NtCreateSection", typeof(DELEGATES.NtCreateSection), ref funcargs);
            if (retValue != Data.Native.NTSTATUS.Success)
            {
                throw new InvalidOperationException("Unable to create section, " + retValue);
            }

            // Update the modified variables
            SectionHandle = (IntPtr) funcargs[0];
            MaximumSize = (ulong) funcargs[3];

            return retValue;
        }

        public static Data.Native.NTSTATUS NtMapViewOfSection(
            IntPtr SectionHandle,
            IntPtr ProcessHandle,
            ref IntPtr BaseAddress,
            IntPtr ZeroBits,
            IntPtr CommitSize,
            IntPtr SectionOffset,
            ref ulong ViewSize,
            uint InheritDisposition,
            uint AllocationType,
            uint Win32Protect)
        {

            // Craft an array for the arguments
            object[] funcargs =
            {
                SectionHandle, ProcessHandle, BaseAddress, ZeroBits, CommitSize, SectionOffset, ViewSize, InheritDisposition, AllocationType,
                Win32Protect
            };

            Data.Native.NTSTATUS retValue = (Data.Native.NTSTATUS)Generic.DynamicAPIInvoke(@"ntdll.dll", @"NtMapViewOfSection", typeof(DELEGATES.NtMapViewOfSection), ref funcargs);
            if (retValue != Data.Native.NTSTATUS.Success && retValue != Data.Native.NTSTATUS.ImageNotAtBase)
            {
                throw new InvalidOperationException("Unable to map view of section, " + retValue);
            }

            // Update the modified variables.
            BaseAddress = (IntPtr) funcargs[2];
            ViewSize = (ulong) funcargs[6];

            return retValue;
        }

        public static void RtlInitUnicodeString(ref Data.Native.UNICODE_STRING DestinationString, [MarshalAs(UnmanagedType.LPWStr)] string SourceString)
        {
            // Craft an array for the arguments
            object[] funcargs =
            {
                DestinationString, SourceString
            };

            Generic.DynamicAPIInvoke(@"ntdll.dll", @"RtlInitUnicodeString", typeof(DELEGATES.RtlInitUnicodeString), ref funcargs);

            // Update the modified variables
            DestinationString = (Data.Native.UNICODE_STRING)funcargs[0];
        }

        public static Data.Native.NTSTATUS LdrLoadDll(IntPtr PathToFile, UInt32 dwFlags, ref Data.Native.UNICODE_STRING ModuleFileName, ref IntPtr ModuleHandle)
        {
            // Craft an array for the arguments
            object[] funcargs =
            {
                PathToFile, dwFlags, ModuleFileName, ModuleHandle
            };

            Data.Native.NTSTATUS retValue = (Data.Native.NTSTATUS)Generic.DynamicAPIInvoke(@"ntdll.dll", @"LdrLoadDll", typeof(DELEGATES.LdrLoadDll), ref funcargs);

            // Update the modified variables
            ModuleHandle = (IntPtr)funcargs[3];

            return retValue;
        }

        public static void RtlZeroMemory(IntPtr Destination, int Length)
        {
            // Craft an array for the arguments
            object[] funcargs =
            {
                Destination, Length
            };

            Generic.DynamicAPIInvoke(@"ntdll.dll", @"RtlZeroMemory", typeof(DELEGATES.RtlZeroMemory), ref funcargs);
        }

        public static Data.Native.NTSTATUS NtQueryInformationProcess(IntPtr hProcess, Data.Native.PROCESSINFOCLASS processInfoClass, out IntPtr pProcInfo)
        {
            int processInformationLength;
            UInt32 RetLen = 0;

            switch (processInfoClass)
            {
                case Data.Native.PROCESSINFOCLASS.ProcessWow64Information:
                    pProcInfo = Marshal.AllocHGlobal(IntPtr.Size);
                    RtlZeroMemory(pProcInfo, IntPtr.Size);
                    processInformationLength = IntPtr.Size;
                    break;
                case Data.Native.PROCESSINFOCLASS.ProcessBasicInformation:
                    Data.Native.PROCESS_BASIC_INFORMATION PBI = new Data.Native.PROCESS_BASIC_INFORMATION();
                    pProcInfo = Marshal.AllocHGlobal(Marshal.SizeOf(PBI));
                    RtlZeroMemory(pProcInfo, Marshal.SizeOf(PBI));
                    Marshal.StructureToPtr(PBI, pProcInfo, true);
                    processInformationLength = Marshal.SizeOf(PBI);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid ProcessInfoClass: {processInfoClass}");
            }

            object[] funcargs =
            {
                hProcess, processInfoClass, pProcInfo, processInformationLength, RetLen
            };

            Data.Native.NTSTATUS retValue = (Data.Native.NTSTATUS)Generic.DynamicAPIInvoke(@"ntdll.dll", @"NtQueryInformationProcess", typeof(DELEGATES.NtQueryInformationProcess), ref funcargs);
            if (retValue != Data.Native.NTSTATUS.Success)
            {
                throw new UnauthorizedAccessException("Access is denied.");
            }

            // Update the modified variables
            pProcInfo = (IntPtr)funcargs[2];

            return retValue;
        }

        public static Data.Native.PROCESS_BASIC_INFORMATION NtQueryInformationProcessBasicInformation(IntPtr hProcess)
        {
            Data.Native.NTSTATUS retValue = NtQueryInformationProcess(hProcess, Data.Native.PROCESSINFOCLASS.ProcessBasicInformation, out IntPtr pProcInfo);
            if (retValue != Data.Native.NTSTATUS.Success)
            {
                throw new UnauthorizedAccessException("Access is denied.");
            }

            return (Data.Native.PROCESS_BASIC_INFORMATION)Marshal.PtrToStructure(pProcInfo, typeof(Data.Native.PROCESS_BASIC_INFORMATION));
        }

        public static UInt32 NtProtectVirtualMemory(IntPtr ProcessHandle, ref IntPtr BaseAddress, ref IntPtr RegionSize, UInt32 NewProtect)
        {
            // Craft an array for the arguments
            UInt32 OldProtect = 0;
            object[] funcargs =
            {
                ProcessHandle, BaseAddress, RegionSize, NewProtect, OldProtect
            };

            Data.Native.NTSTATUS retValue = (Data.Native.NTSTATUS)Generic.DynamicAPIInvoke(@"ntdll.dll", @"NtProtectVirtualMemory", typeof(DELEGATES.NtProtectVirtualMemory), ref funcargs);
            if (retValue != Data.Native.NTSTATUS.Success)
            {
                throw new InvalidOperationException("Failed to change memory protection, " + retValue);
            }

            OldProtect = (UInt32)funcargs[4];
            return OldProtect;
        }

        public static UInt32 NtWriteVirtualMemory(IntPtr ProcessHandle, IntPtr BaseAddress, IntPtr Buffer, UInt32 BufferLength)
        {
            // Craft an array for the arguments
            UInt32 BytesWritten = 0;
            object[] funcargs =
            {
                ProcessHandle, BaseAddress, Buffer, BufferLength, BytesWritten
            };

            Data.Native.NTSTATUS retValue = (Data.Native.NTSTATUS)Generic.DynamicAPIInvoke(@"ntdll.dll", @"NtWriteVirtualMemory", typeof(DELEGATES.NtWriteVirtualMemory), ref funcargs);
            if (retValue != Data.Native.NTSTATUS.Success)
            {
                throw new InvalidOperationException("Failed to write memory, " + retValue);
            }

            BytesWritten = (UInt32)funcargs[4];
            return BytesWritten;
        }

        public static IntPtr LdrGetProcedureAddress(IntPtr hModule, IntPtr FunctionName, IntPtr Ordinal, ref IntPtr FunctionAddress)
        {
            // Craft an array for the arguments
            object[] funcargs =
            {
                hModule, FunctionName, Ordinal, FunctionAddress
            };

            Data.Native.NTSTATUS retValue = (Data.Native.NTSTATUS)Generic.DynamicAPIInvoke(@"ntdll.dll", @"LdrGetProcedureAddress", typeof(DELEGATES.LdrGetProcedureAddress), ref funcargs);
            if (retValue != Data.Native.NTSTATUS.Success)
            {
                throw new InvalidOperationException("Failed get procedure address, " + retValue);
            }

            FunctionAddress = (IntPtr)funcargs[3];
            return FunctionAddress;
        }

        public static void RtlGetVersion(ref Data.Native.OSVERSIONINFOEX VersionInformation)
        {
            // Craft an array for the arguments
            object[] funcargs =
            {
                VersionInformation
            };

            Data.Native.NTSTATUS retValue = (Data.Native.NTSTATUS)Generic.DynamicAPIInvoke(@"ntdll.dll", @"RtlGetVersion", typeof(DELEGATES.RtlGetVersion), ref funcargs);
            if (retValue != Data.Native.NTSTATUS.Success)
            {
                throw new InvalidOperationException("Failed get procedure address, " + retValue);
            }

            VersionInformation = (Data.Native.OSVERSIONINFOEX)funcargs[0];
        }

        public static IntPtr NtOpenFile(ref IntPtr FileHandle, Data.Win32.Kernel32.FileAccessFlags DesiredAccess, ref Data.Native.OBJECT_ATTRIBUTES ObjAttr, ref Data.Native.IO_STATUS_BLOCK IoStatusBlock, Data.Win32.Kernel32.FileShareFlags ShareAccess, Data.Win32.Kernel32.FileOpenFlags OpenOptions)
        {
            // Craft an array for the arguments
            object[] funcargs =
            {
                FileHandle, DesiredAccess, ObjAttr, IoStatusBlock, ShareAccess, OpenOptions
            };

            Data.Native.NTSTATUS retValue = (Data.Native.NTSTATUS)Generic.DynamicAPIInvoke(@"ntdll.dll", @"NtOpenFile", typeof(DELEGATES.NtOpenFile), ref funcargs);
            if (retValue != Data.Native.NTSTATUS.Success)
            {
                throw new InvalidOperationException("Failed to open file, " + retValue);
            }


            FileHandle = (IntPtr)funcargs[0];
            return FileHandle;
        }

        public struct DELEGATES
        {

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate Data.Native.NTSTATUS NtCreateSection(
                ref IntPtr SectionHandle,
                uint DesiredAccess,
                IntPtr ObjectAttributes,
                ref ulong MaximumSize,
                uint SectionPageProtection,
                uint AllocationAttributes,
                IntPtr FileHandle);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate Data.Native.NTSTATUS NtMapViewOfSection(
                IntPtr SectionHandle,
                IntPtr ProcessHandle,
                out IntPtr BaseAddress,
                IntPtr ZeroBits,
                IntPtr CommitSize,
                IntPtr SectionOffset,
                out ulong ViewSize,
                uint InheritDisposition,
                uint AllocationType,
                uint Win32Protect);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate UInt32 LdrLoadDll(
                IntPtr PathToFile,
                UInt32 dwFlags,
                ref Data.Native.UNICODE_STRING ModuleFileName,
                ref IntPtr ModuleHandle);
            
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void RtlInitUnicodeString(
                ref Data.Native.UNICODE_STRING DestinationString,
                [MarshalAs(UnmanagedType.LPWStr)]
                string SourceString);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void RtlZeroMemory(
                IntPtr Destination,
                int length);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate UInt32 NtQueryInformationProcess(
                IntPtr processHandle,
                Data.Native.PROCESSINFOCLASS processInformationClass,
                IntPtr processInformation,
                int processInformationLength,
                ref UInt32 returnLength);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate UInt32 NtProtectVirtualMemory(
                IntPtr ProcessHandle,
                ref IntPtr BaseAddress,
                ref IntPtr RegionSize,
                UInt32 NewProtect,
                ref UInt32 OldProtect);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate UInt32 NtWriteVirtualMemory(
                IntPtr ProcessHandle,
                IntPtr BaseAddress,
                IntPtr Buffer,
                UInt32 BufferLength,
                ref UInt32 BytesWritten);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate UInt32 LdrGetProcedureAddress(
                IntPtr hModule,
                IntPtr FunctionName,
                IntPtr Ordinal,
                ref IntPtr FunctionAddress);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate UInt32 RtlGetVersion(
                ref Data.Native.OSVERSIONINFOEX VersionInformation);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate UInt32 NtOpenFile(
                ref IntPtr FileHandle,
                Data.Win32.Kernel32.FileAccessFlags DesiredAccess,
                ref Data.Native.OBJECT_ATTRIBUTES ObjAttr,
                ref Data.Native.IO_STATUS_BLOCK IoStatusBlock,
                Data.Win32.Kernel32.FileShareFlags ShareAccess,
                Data.Win32.Kernel32.FileOpenFlags OpenOptions);
        }
    }
}