using System;
using System.Runtime.InteropServices;

using DInvoke.DynamicInvoke;

namespace Drone.Utilities;

public static class Native
{
    public static IntPtr NtOpenProcess(uint pid, uint desiredAccess)
    {
        var hProcess = IntPtr.Zero;

        var oa = new OBJECT_ATTRIBUTES();
        var ci = new CLIENT_ID { UniqueProcess = (IntPtr)pid };

        object[] parameters = { hProcess, desiredAccess, oa, ci };

        Generic.DynamicApiInvoke(
            "ntdll.dll",
            "NtOpenProcess",
            typeof(Delegates.NtOpenProcess),
            ref parameters);

        hProcess = (IntPtr)parameters[0];
        return hProcess;
    }

    public static void RtlZeroMemory(IntPtr destination, int length)
    {
        object[] parameters = { destination, length };

        Generic.DynamicApiInvoke(
            "ntdll.dll",
            "RtlZeroMemory",
            typeof(Delegates.RtlZeroMemory),
            ref parameters);
    }

    public static PROCESS_BASIC_INFORMATION QueryProcessBasicInformation(IntPtr hProcess)
    {
        NtQueryInformationProcess(
            hProcess,
            PROCESS_INFO_CLASS.ProcessBasicInformation,
            out var pProcInfo);

        return (PROCESS_BASIC_INFORMATION)Marshal.PtrToStructure(pProcInfo, typeof(PROCESS_BASIC_INFORMATION));
    }
    
    public static bool NtQueryInformationProcessWow64Information(IntPtr hProcess)
    {
        var result = NtQueryInformationProcess(
            hProcess,
            PROCESS_INFO_CLASS.ProcessWow64Information,
            out var pProcInfo);

        if (result != 0)
            throw new UnauthorizedAccessException("Access is denied.");

        return Marshal.ReadIntPtr(pProcInfo) != IntPtr.Zero;
    }
    
    public static uint NtQueryInformationProcess(IntPtr hProcess, PROCESS_INFO_CLASS processInfoClass, out IntPtr pProcInfo)
    {
        int processInformationLength;
        uint retLen = 0;

        switch (processInfoClass)
        {
            case PROCESS_INFO_CLASS.ProcessWow64Information:
                pProcInfo = Marshal.AllocHGlobal(IntPtr.Size);
                RtlZeroMemory(pProcInfo, IntPtr.Size);
                processInformationLength = IntPtr.Size;
                break;

            case PROCESS_INFO_CLASS.ProcessBasicInformation:
                var pbi = new PROCESS_BASIC_INFORMATION();
                pProcInfo = Marshal.AllocHGlobal(Marshal.SizeOf(pbi));
                RtlZeroMemory(pProcInfo, Marshal.SizeOf(pbi));
                Marshal.StructureToPtr(pbi, pProcInfo, true);
                processInformationLength = Marshal.SizeOf(pbi);
                break;

            default:
                throw new InvalidOperationException($"Invalid ProcessInfoClass: {processInfoClass}");
        }

        object[] parameters = { hProcess, processInfoClass, pProcInfo, processInformationLength, retLen };

        var status = (uint)Generic.DynamicApiInvoke(
            "ntdll.dll",
            "NtQueryInformationProcess",
            typeof(Delegates.NtQueryInformationProcess),
            ref parameters);

        pProcInfo = (IntPtr)parameters[2];
        return status;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct OBJECT_ATTRIBUTES
    {
        public int Length;
        public IntPtr RootDirectory;
        public IntPtr ObjectName;
        public uint Attributes;
        public IntPtr SecurityDescriptor;
        public IntPtr SecurityQualityOfService;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct CLIENT_ID
    {
        public IntPtr UniqueProcess;
        public IntPtr UniqueThread;
    }
    
    public enum PROCESS_INFO_CLASS
    {
        ProcessBasicInformation = 0, // 0, q: PROCESS_BASIC_INFORMATION, PROCESS_EXTENDED_BASIC_INFORMATION
        ProcessQuotaLimits, // qs: QUOTA_LIMITS, QUOTA_LIMITS_EX
        ProcessIoCounters, // q: IO_COUNTERS
        ProcessVmCounters, // q: VM_COUNTERS, VM_COUNTERS_EX
        ProcessTimes, // q: KERNEL_USER_TIMES
        ProcessBasePriority, // s: KPRIORITY
        ProcessRaisePriority, // s: ULONG
        ProcessDebugPort, // q: HANDLE
        ProcessExceptionPort, // s: HANDLE
        ProcessAccessToken, // s: PROCESS_ACCESS_TOKEN
        ProcessLdtInformation, // 10
        ProcessLdtSize,
        ProcessDefaultHardErrorMode, // qs: ULONG
        ProcessIoPortHandlers, // (kernel-mode only)
        ProcessPooledUsageAndLimits, // q: POOLED_USAGE_AND_LIMITS
        ProcessWorkingSetWatch, // q: PROCESS_WS_WATCH_INFORMATION[]; s: void
        ProcessUserModeIOPL,
        ProcessEnableAlignmentFaultFixup, // s: BOOLEAN
        ProcessPriorityClass, // qs: PROCESS_PRIORITY_CLASS
        ProcessWx86Information,
        ProcessHandleCount, // 20, q: ULONG, PROCESS_HANDLE_INFORMATION
        ProcessAffinityMask, // s: KAFFINITY
        ProcessPriorityBoost, // qs: ULONG
        ProcessDeviceMap, // qs: PROCESS_DEVICEMAP_INFORMATION, PROCESS_DEVICEMAP_INFORMATION_EX
        ProcessSessionInformation, // q: PROCESS_SESSION_INFORMATION
        ProcessForegroundInformation, // s: PROCESS_FOREGROUND_BACKGROUND
        ProcessWow64Information, // q: ULONG_PTR
        ProcessImageFileName, // q: UNICODE_STRING
        ProcessLUIDDeviceMapsEnabled, // q: ULONG
        ProcessBreakOnTermination, // qs: ULONG
        ProcessDebugObjectHandle, // 30, q: HANDLE
        ProcessDebugFlags, // qs: ULONG
        ProcessHandleTracing, // q: PROCESS_HANDLE_TRACING_QUERY; s: size 0 disables, otherwise enables
        ProcessIoPriority, // qs: ULONG
        ProcessExecuteFlags, // qs: ULONG
        ProcessResourceManagement,
        ProcessCookie, // q: ULONG
        ProcessImageInformation, // q: SECTION_IMAGE_INFORMATION
        ProcessCycleTime, // q: PROCESS_CYCLE_TIME_INFORMATION
        ProcessPagePriority, // q: ULONG
        ProcessInstrumentationCallback, // 40
        ProcessThreadStackAllocation, // s: PROCESS_STACK_ALLOCATION_INFORMATION, PROCESS_STACK_ALLOCATION_INFORMATION_EX
        ProcessWorkingSetWatchEx, // q: PROCESS_WS_WATCH_INFORMATION_EX[]
        ProcessImageFileNameWin32, // q: UNICODE_STRING
        ProcessImageFileMapping, // q: HANDLE (input)
        ProcessAffinityUpdateMode, // qs: PROCESS_AFFINITY_UPDATE_MODE
        ProcessMemoryAllocationMode, // qs: PROCESS_MEMORY_ALLOCATION_MODE
        ProcessGroupInformation, // q: USHORT[]
        ProcessTokenVirtualizationEnabled, // s: ULONG
        ProcessConsoleHostProcess, // q: ULONG_PTR
        ProcessWindowInformation, // 50, q: PROCESS_WINDOW_INFORMATION
        ProcessHandleInformation, // q: PROCESS_HANDLE_SNAPSHOT_INFORMATION // since WIN8
        ProcessMitigationPolicy, // s: PROCESS_MITIGATION_POLICY_INFORMATION
        ProcessDynamicFunctionTableInformation,
        ProcessHandleCheckingMode,
        ProcessKeepAliveCount, // q: PROCESS_KEEPALIVE_COUNT_INFORMATION
        ProcessRevokeFileHandles, // s: PROCESS_REVOKE_FILE_HANDLES_INFORMATION
        MaxProcessInfoClass
    }
    
    public struct PROCESS_BASIC_INFORMATION
    {
        public IntPtr ExitStatus;
        public IntPtr PebBaseAddress;
        public IntPtr AffinityMask;
        public IntPtr BasePriority;
        public UIntPtr UniqueProcessId;
        public int InheritedFromUniqueProcessId;

        public int Size => Marshal.SizeOf(typeof(PROCESS_BASIC_INFORMATION));
    }
}