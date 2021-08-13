// Author: Ryan Cobb (@cobbr_io), The Wover (@TheRealWover)
// Project: SharpSploit (https://github.com/cobbr/SharpSploit)
// License: BSD 3-Clause

using System;
using System.Runtime.InteropServices;

namespace Drone.DInvoke.DynamicInvoke
{
    public static class Win32
    {
        public static bool LogonUserA(string lpszUsername, string lpszDomain, string lpszPassword,
            Data.Win32.Advapi32.LogonUserType dwLogonType, Data.Win32.Advapi32.LogonUserProvider dwLogonProvider,
            ref IntPtr phToken)
        {
            object[] funcargs =
            {
                lpszUsername,
                lpszDomain,
                lpszPassword,
                dwLogonType,
                dwLogonProvider,
                phToken
            };

            var retVal = (bool)Generic.DynamicAPIInvoke(@"advapi32.dll", @"LogonUserA",
                typeof(Delegates.LogonUserA), ref funcargs);

            phToken = (IntPtr) funcargs[5];
            return retVal;
        }

        public static bool ImpersonateLoggedOnUser(IntPtr hToken)
        {
            object[] funcargs = { hToken };
            
            var retVal = (bool)Generic.DynamicAPIInvoke(@"advapi32.dll", @"ImpersonateLoggedOnUser",
                typeof(Delegates.ImpersonateLoggedOnUser), ref funcargs);
            
            return retVal;
        }

        public static bool RevertToSelf()
        {
            object[] funcargs = { };
            
            var retVal = (bool)Generic.DynamicAPIInvoke(@"advapi32.dll", @"RevertToSelf",
                typeof(Delegates.RevertToSelf), ref funcargs);
            
            return retVal;
        }

        public static bool OpenProcessToken(IntPtr hProcess, Data.Win32.Advapi32.TokenAccess tokenAccess, ref IntPtr hToken)
        {
            object[] funcargs = { hProcess, tokenAccess, hToken };
            
            var retVal = (bool)Generic.DynamicAPIInvoke(@"advapi32.dll", @"OpenProcessToken",
                typeof(Delegates.OpenProcessToken), ref funcargs);

            hToken = (IntPtr) funcargs[2];
            
            return retVal;
        }

        public static bool DuplicateTokenEx(IntPtr hExistingToken, Data.Win32.Advapi32.TokenAccess dwDesiredAccess,
            Data.Win32.WinNT.SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
            Data.Win32.WinNT.TOKEN_TYPE TokenType, ref IntPtr phNewToken)
        {
            var lpTokenAttributes = new Data.Win32.WinNT.SECURITY_ATTRIBUTES();
            lpTokenAttributes.nLength = Marshal.SizeOf(lpTokenAttributes);
            
            object[] funcargs =
            {
                hExistingToken, dwDesiredAccess, lpTokenAttributes, ImpersonationLevel,
                TokenType, phNewToken
            };
            
            var retVal = (bool)Generic.DynamicAPIInvoke(@"advapi32.dll", @"DuplicateTokenEx",
                typeof(Delegates.DuplicateTokenEx), ref funcargs);

            phNewToken = (IntPtr) funcargs[5];
            
            return retVal;
        }
        
        public static bool CloseHandle(IntPtr handle)
        {
            object[] funcargs = { handle };
            
            var retVal = (bool)Generic.DynamicAPIInvoke(@"kernel32.dll", @"CloseHandle",
                typeof(Delegates.CloseHandle), ref funcargs);
            
            return retVal;
        }

        private static class Delegates
        {
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool CloseHandle(IntPtr handle);
            
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool LogonUserA(
                string lpszUsername,
                string lpszDomain,
                string lpszPassword,
                Data.Win32.Advapi32.LogonUserType dwLogonType,
                Data.Win32.Advapi32.LogonUserProvider dwLogonProvider,
                ref IntPtr phToken);
            
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool ImpersonateLoggedOnUser(IntPtr hToken);
            
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool OpenProcessToken(
                IntPtr ProcessHandle,
                Data.Win32.Advapi32.TokenAccess DesiredAccess,
                ref IntPtr TokenHandle);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool DuplicateTokenEx(
                IntPtr hExistingToken,
                Data.Win32.Advapi32.TokenAccess dwDesiredAccess,
                ref Data.Win32.WinNT.SECURITY_ATTRIBUTES lpTokenAttributes,
                Data.Win32.WinNT.SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
                Data.Win32.WinNT.TOKEN_TYPE TokenType,
                ref IntPtr phNewToken);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool RevertToSelf();
        }
    }
}