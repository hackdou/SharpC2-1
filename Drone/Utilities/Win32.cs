using System;
using System.Runtime.InteropServices;

using DInvoke.DynamicInvoke;

namespace Drone.Utilities;

public static class Win32
{
    public const uint SC_MANAGER_ALL_ACCESS = 0xF003F;
    public const uint SERVICE_ALL_ACCESS = 0xF01FF;
    public const uint SERVICE_WIN32_OWN_PROCESS = 0x00000010;
    public const uint SERVICE_DEMAND_START = 0x00000003;
    public const uint SERVICE_ERROR_IGNORE = 0x00000000;

    public static IntPtr LogonUserW(string username, string domain, string password, LOGON_USER_TYPE logonType,
        LOGON_USER_PROVIDER logonProvider)
    {
        var hToken = IntPtr.Zero;
        object[] parameters = { username, domain, password, logonType,logonProvider, hToken };
        
        Generic.DynamicApiInvoke(
            "advapi32.dll",
            "LogonUserW",
            typeof(Delegates.LogonUserW),
            ref parameters);
        
        hToken = (IntPtr)parameters[5];
        return hToken;
    }

    public static bool ImpersonateToken(IntPtr hToken)
    {
        object[] parameters = { hToken };
        return (bool)Generic.DynamicApiInvoke(
            "advapi32.dll",
            "ImpersonateLoggedOnUser",
            typeof(Delegates.ImpersonateLoggedOnUser),
            ref parameters);
    }

    public static IntPtr OpenProcessToken(IntPtr hProcess, TOKEN_ACCESS tokenAccess)
    {
        var hToken = IntPtr.Zero;
        object[] parameters = { hProcess, tokenAccess, hToken };

        Generic.DynamicApiInvoke(
            "advapi32.dll",
            "OpenProcessToken",
            typeof(Delegates.OpenProcessToken),
            ref parameters);

        hToken = (IntPtr)parameters[2];
        return hToken;
    }
    
    public static IntPtr DuplicateTokenEx(IntPtr hExistingToken, TOKEN_ACCESS tokenAccess,
        SECURITY_IMPERSONATION_LEVEL impersonationLevel, TOKEN_TYPE tokenType)
    {
        var hNewToken = IntPtr.Zero;
        
        var lpTokenAttributes = new SECURITY_ATTRIBUTES();
        lpTokenAttributes.nLength = Marshal.SizeOf(lpTokenAttributes);

        object[] parameters =
        {
            hExistingToken, tokenAccess, lpTokenAttributes, impersonationLevel,
            tokenType, hNewToken
        };

        Generic.DynamicApiInvoke(
            "advapi32.dll",
            "DuplicateTokenEx",
            typeof(Delegates.DuplicateTokenEx),
            ref parameters);

        hNewToken = (IntPtr)parameters[5];
        return hNewToken;
    }
    
    public static bool RevertToSelf()
    {
        object[] parameters = { };

        return (bool)Generic.DynamicApiInvoke(
            "advapi32.dll",
            "RevertToSelf",
            typeof(Delegates.RevertToSelf),
            ref parameters);
    }

    public static void CloseServiceHandle(IntPtr hObject)
    {
        object[] parameters = { hObject };
        
        Generic.DynamicApiInvoke(
            "advapi32.dll",
            "CloseServiceHandle",
            typeof(Delegates.CloseHandle),
            ref parameters);
    }
    
    public static void CloseHandle(IntPtr hObject)
    {
        object[] parameters = { hObject };
        
        Generic.DynamicApiInvoke(
            "kernel32.dll",
            "CloseHandle",
            typeof(Delegates.CloseHandle),
            ref parameters);
    }

    public enum LOGON_USER_TYPE
    {
        LOGON32_LOGON_INTERACTIVE = 2,
        LOGON32_LOGON_NETWORK = 3,
        LOGON32_LOGON_BATCH = 4,
        LOGON32_LOGON_SERVICE = 5,
        LOGON32_LOGON_UNLOCK = 7,
        LOGON32_LOGON_NETWORK_CLEARTEXT = 8,
        LOGON32_LOGON_NEW_CREDENTIALS = 9
    }
        
    public enum LOGON_USER_PROVIDER
    {
        LOGON32_PROVIDER_DEFAULT = 0,
        LOGON32_PROVIDER_WINNT35 = 1,
        LOGON32_PROVIDER_WINNT40 = 2,
        LOGON32_PROVIDER_WINNT50 = 3,
        LOGON32_PROVIDER_VIRTUAL = 4
    }
    
    [Flags]
    public enum PROCESS_ACCESS_FLAGS : uint
    {
        PROCESS_ALL_ACCESS = 0x001F0FFF,
        PROCESS_CREATE_PROCESS = 0x0080,
        PROCESS_CREATE_THREAD = 0x0002,
        PROCESS_DUP_HANDLE = 0x0040,
        PROCESS_QUERY_INFORMATION = 0x0400,
        PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
        PROCESS_SET_INFORMATION = 0x0200,
        PROCESS_SET_QUOTA = 0x0100,
        PROCESS_SUSPEND_RESUME = 0x0800,
        PROCESS_TERMINATE = 0x0001,
        PROCESS_VM_OPERATION = 0x0008,
        PROCESS_VM_READ = 0x0010,
        PROCESS_VM_WRITE = 0x0020,
        SYNCHRONIZE = 0x00100000
    }
    
    [Flags]
    public enum TOKEN_ACCESS : uint
    {
        TOKEN_ASSIGN_PRIMARY = 0x0001,
        TOKEN_DUPLICATE = 0x0002,
        TOKEN_IMPERSONATE = 0x0004,
        TOKEN_QUERY = 0x0008,
        TOKEN_QUERY_SOURCE = 0x0010,
        TOKEN_ADJUST_PRIVILEGES = 0x0020,
        TOKEN_ADJUST_GROUPS = 0x0040,
        TOKEN_ADJUST_DEFAULT = 0x0080,
        TOKEN_ADJUST_SESSIONID = 0x0100,
        TOKEN_ALL_ACCESS_P = 0x000F00FF,
        TOKEN_ALL_ACCESS = 0x000F01FF,
        TOKEN_READ = 0x00020008,
        TOKEN_WRITE = 0x000200E0,
        TOKEN_EXECUTE = 0x00020000
    }
    
    public enum SECURITY_IMPERSONATION_LEVEL
    {
        SECURITY_ANONYMOUS,
        SECURITY_IDENTIFICATION,
        SECURITY_IMPERSONATION,
        SECURITY_DELEGATION
    }
    
    public enum TOKEN_TYPE
    {
        TOKEN_PRIMARY = 1,
        TOKEN_IMPERSONATION = 2
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
        public int nLength;
        public IntPtr lpSecurityDescriptor;
        public int bInheritHandle;
    }
}