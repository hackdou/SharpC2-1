using System;
using System.Runtime.InteropServices;

namespace Drone.Utilities;

public static class Delegates
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
    public delegate IntPtr OpenScManagerW(
        [MarshalAs(UnmanagedType.LPWStr)] string lpMachineName,
        [MarshalAs(UnmanagedType.LPWStr)] string lpDatabaseName,
        uint dwDesiredAccess);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
    public delegate IntPtr CreateServiceW(
        IntPtr hScManager,
        [MarshalAs(UnmanagedType.LPWStr)] string lpServiceName,
        [MarshalAs(UnmanagedType.LPWStr)] string lpDisplayName,
        uint dwDesiredAccess,
        uint dwServiceType,
        uint dwStartType,
        uint dwErrorControl,
        [MarshalAs(UnmanagedType.LPWStr)] string lpBinaryPathName,
        [MarshalAs(UnmanagedType.LPWStr)] string lpLoadOrderGroup,
        uint lpdwTagId,
        [MarshalAs(UnmanagedType.LPWStr)] string lpDependencies,
        [MarshalAs(UnmanagedType.LPWStr)] string lpServiceStartName,
        [MarshalAs(UnmanagedType.LPWStr)] string lpPassword);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool StartServiceW(
        IntPtr hService,
        uint dwNumServiceArgs,
        string[] lpServiceArgVectors);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool DeleteService(IntPtr hService);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool CloseHandle(IntPtr hObject);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
    public delegate bool LogonUserW(
        [MarshalAs(UnmanagedType.LPWStr)] string lpszUsername,
        [MarshalAs(UnmanagedType.LPWStr)] string lpszDomain,
        [MarshalAs(UnmanagedType.LPWStr)] string lpszPassword,
        Win32.LOGON_USER_TYPE dwLogonType,
        Win32.LOGON_USER_PROVIDER dwLogonProvider,
        out IntPtr phToken);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool ImpersonateLoggedOnUser(IntPtr hToken);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtOpenProcess(
        ref IntPtr processHandle,
        uint desiredAccess,
        ref Native.OBJECT_ATTRIBUTES objectAttributes,
        ref Native.CLIENT_ID clientId);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool OpenProcessToken(
        IntPtr processHandle,
        Win32.TOKEN_ACCESS desiredAccess,
        out IntPtr tokenHandle);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool DuplicateTokenEx(
        IntPtr hExistingToken,
        Win32.TOKEN_ACCESS dwDesiredAccess,
        ref Win32.SECURITY_ATTRIBUTES lpTokenAttributes,
        Win32.SECURITY_IMPERSONATION_LEVEL impersonationLevel,
        Win32.TOKEN_TYPE tokenType,
        out IntPtr phNewToken);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool RevertToSelf();

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void RtlZeroMemory(
        IntPtr destination,
        int length);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtQueryInformationProcess(
        IntPtr processHandle,
        Native.PROCESS_INFO_CLASS processInformationClass,
        IntPtr processInformation,
        int processInformationLength,
        ref uint returnLength);
}