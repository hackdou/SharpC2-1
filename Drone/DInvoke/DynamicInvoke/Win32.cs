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

        public static IntPtr CreateFileW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpFileName, uint dwDesiredAccess, uint dwShareMode,
            IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile)
        {
            object[] funcargs =
            {
                lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
                dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile
            };
            
            var retVal = (IntPtr)Generic.DynamicAPIInvoke(@"KernelBase.dll", @"CreateFileW",
                typeof(Delegates.CreateFileW), ref funcargs);
            
            return retVal;
        }

        public static uint GetFileAttributesW(IntPtr lpFileName)
        {
            object[] funcargs = { lpFileName };
            
            var retVal = (uint)Generic.DynamicAPIInvoke(@"KernelBase.dll", @"GetFileAttributesW",
                typeof(Delegates.GetFileAttributesW), ref funcargs);
            
            return retVal;
        }

        public static bool GetFileAttributesExW(IntPtr lpFileName, uint fInfoLevelId, ref IntPtr lpFileInformation)
        {
            object[] funcargs = { lpFileName, fInfoLevelId, lpFileInformation };
            
            var retVal = (bool)Generic.DynamicAPIInvoke(@"KernelBase.dll", @"GetFileAttributesExW",
                typeof(Delegates.GetFileAttributesExW), ref funcargs);

            lpFileInformation = (IntPtr)funcargs[2];
            
            return retVal;
        }

        public static bool GetFileInformationByHandle(IntPtr hFile, ref IntPtr lpFileInformation)
        {
            object[] funcargs = { hFile, lpFileInformation };
            
            var retVal = (bool)Generic.DynamicAPIInvoke(@"KernelBase.dll", @"GetFileInformationByHandle",
                typeof(Delegates.GetFileInformationByHandle), ref funcargs);

            lpFileInformation = (IntPtr)funcargs[1];
            
            return retVal;
        }

        public static IntPtr CreateTransaction(IntPtr lpTransactionAttributes, IntPtr uow, int createOptions,
            int isolationLevel, int isolationFlags, int timeout,
            [MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder description)
        {
            object[] funcargs =
            {
                lpTransactionAttributes, uow, createOptions, isolationLevel, isolationFlags,
                timeout, description
            };
            
            var retVal = (IntPtr)Generic.DynamicAPIInvoke(@"ktmw32.dll", @"CreateTransaction",
                typeof(Delegates.CreateTransaction), ref funcargs, true);

            return retVal;
        }

        public static IntPtr CreateFileTransactedW([MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
            uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition,
            uint dwFlagsAndAttributes, IntPtr hTemplateFile, IntPtr hTransaction, ref ushort pusMiniVersion,
            IntPtr nullValue)
        {
            object[] funcargs =
            {
                lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition,
                dwFlagsAndAttributes, hTemplateFile, hTransaction, pusMiniVersion, nullValue
            };
            
            var retVal = (IntPtr)Generic.DynamicAPIInvoke(@"Kernel32.dll", @"CreateFileTransactedW",
                typeof(Delegates.CreateFileTransactedW), ref funcargs);

            return retVal;
        }

        public static bool WriteFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite,
            ref uint lpNumberOfBytesWritten, IntPtr lpOverlapped)
        {
            object[] funcargs =
            {
                hFile, lpBuffer, nNumberOfBytesToWrite, lpNumberOfBytesWritten, lpOverlapped
            };
            
            var retVal = (bool)Generic.DynamicAPIInvoke(@"Kernel32.dll", @"WriteFile",
                typeof(Delegates.WriteFile), ref funcargs);

            return retVal;
        }

        public static class Delegates
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
            
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate IntPtr CreateFileW(
                [MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
                uint dwDesiredAccess,
                uint dwShareMode,
                IntPtr lpSecurityAttributes,
                uint dwCreationDisposition,
                uint dwFlagsAndAttributes,
                IntPtr hTemplateFile);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint GetFileAttributesW(IntPtr lpFileName);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool GetFileAttributesExW(
                IntPtr lpFileName,
                uint fInfoLevelId,
                IntPtr lpFileInformation);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool GetFileInformationByHandle(
                IntPtr hFile,
                IntPtr lpFileInformation);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate IntPtr CreateTransaction(
                IntPtr lpTransactionAttributes,
                IntPtr uow,
                int createOptions,
                int isolationLevel,
                int isolationFlags,
                int timeout,
                [MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder description);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate IntPtr CreateFileTransactedW(
                [MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
                uint dwDesiredAccess,
                uint dwShareMode,
                IntPtr lpSecurityAttributes,
                uint dwCreationDisposition,
                uint dwFlagsAndAttributes,
                IntPtr hTemplateFile,
                IntPtr hTransaction,
                ref ushort pusMiniVersion,
                IntPtr nullValue);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool WriteFile(
                IntPtr hFile,
                byte[] lpBuffer,
                uint nNumberOfBytesToWrite,
                ref uint lpNumberOfBytesWritten,
                IntPtr lpOverlapped);
        }
    }
}