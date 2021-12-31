using System.Runtime.InteropServices;

namespace TokenModule.Invocation.DynamicInvoke;

public static class Win32
{
    public static class Kernel32
    {
        public static bool CloseHandle(IntPtr handle)
        {
            object[] parameters = { handle };

            var retVal = (bool)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("kernel32.dll",
                "CloseHandle", typeof(Delegates.CloseHandle), ref parameters);

            return retVal;
        }
    }
    
    public static class Advapi32
    {
        public static bool LogonUserA(string lpszUsername, string lpszDomain, string lpszPassword,
            Data.Win32.Advapi32.LogonUserType dwLogonType, Data.Win32.Advapi32.LogonUserProvider dwLogonProvider,
            ref IntPtr phToken)
        {
            object[] parameters =
            {
                lpszUsername, lpszDomain, lpszPassword, dwLogonType, dwLogonProvider, phToken
            };

            var result = (bool)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("advapi32.dll",
                "LogonUserA", typeof(Delegates.LogonUserA), ref parameters);

            phToken = (IntPtr)parameters[5];
            return result;
        }
        
        public static bool ImpersonateLoggedOnUser(IntPtr hToken)
        {
            object[] parameters = { hToken };

            var retVal = (bool)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("advapi32.dll",
                "ImpersonateLoggedOnUser", typeof(Delegates.ImpersonateLoggedOnUser), ref parameters);

            return retVal;
        }
        
        public static bool OpenProcessToken(IntPtr hProcess, Data.Win32.Advapi32.TokenAccess tokenAccess,
            ref IntPtr hToken)
        {
            object[] parameters = { hProcess, tokenAccess, hToken };

            var retVal = (bool)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("advapi32.dll",
                "OpenProcessToken", typeof(Delegates.OpenProcessToken), ref parameters);

            hToken = (IntPtr)parameters[2];
            return retVal;
        }

        public static bool DuplicateTokenEx(IntPtr hExistingToken, Data.Win32.Advapi32.TokenAccess dwDesiredAccess,
            Data.Win32.WinNT.SECURITY_IMPERSONATION_LEVEL impersonationLevel, Data.Win32.WinNT.TOKEN_TYPE tokenType,
            ref IntPtr phNewToken)
        {
            var lpTokenAttributes = new Data.Win32.WinNT.SECURITY_ATTRIBUTES();
            lpTokenAttributes.nLength = Marshal.SizeOf(lpTokenAttributes);

            object[] parameters =
            {
                hExistingToken, dwDesiredAccess, lpTokenAttributes, impersonationLevel,
                tokenType, phNewToken
            };

            var result = (bool)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("advapi32.dll",
                "DuplicateTokenEx", typeof(Delegates.DuplicateTokenEx), ref parameters);

            phNewToken = (IntPtr)parameters[5];
            return result;
        }

        public static bool RevertToSelf()
        {
            object[] parameters = { };

            var retVal = (bool)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("advapi32.dll",
                "RevertToSelf", typeof(Delegates.RevertToSelf), ref parameters);

            return retVal;
        }
    }

    private struct Delegates
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool LogonUserA(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            Data.Win32.Advapi32.LogonUserType dwLogonType,
            Data.Win32.Advapi32.LogonUserProvider dwLogonProvider,
            ref IntPtr phToken);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate bool ImpersonateLoggedOnUser(IntPtr hToken);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool OpenProcessToken(
            IntPtr processHandle,
            Data.Win32.Advapi32.TokenAccess desiredAccess,
            ref IntPtr tokenHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool DuplicateTokenEx(
            IntPtr hExistingToken,
            Data.Win32.Advapi32.TokenAccess dwDesiredAccess,
            ref Data.Win32.WinNT.SECURITY_ATTRIBUTES lpTokenAttributes,
            Data.Win32.WinNT.SECURITY_IMPERSONATION_LEVEL impersonationLevel,
            Data.Win32.WinNT.TOKEN_TYPE tokenType,
            ref IntPtr phNewToken);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate bool RevertToSelf();
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate bool CloseHandle(IntPtr handle);
    }
}