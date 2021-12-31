using System;
using System.Runtime.InteropServices;

namespace StandardModule.Invocation.DynamicInvoke
{
    public static class Win32
    {
        public static class Kernel32
        {
            public static bool IsWow64Process(IntPtr hProcess, ref bool lpSystemInfo)
            {
                object[] parameters = { hProcess, lpSystemInfo };

                var result = (bool)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("kernel32.dll",
                    "IsWow64Process", typeof(Delegates.IsWow64Process), ref parameters);

                lpSystemInfo = (bool)parameters[1];
                return result;
            }

            public static IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize,
                IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, ref IntPtr lpThreadId)
            {
                object[] parameters =
                {
                    hProcess, lpThreadAttributes, dwStackSize, lpStartAddress,
                    lpParameter, dwCreationFlags, lpThreadId
                };

                var result = (IntPtr)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("kernel32.dll",
                    "CreateRemoteThread", typeof(Delegates.CreateRemoteThread), ref parameters);

                lpThreadId = (IntPtr)parameters[6];
                return result;
            }

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

            public static IntPtr OpenScManager(string machineName, string databaseName,
                Data.Win32.Advapi32.SCMAccess dwAccess)
            {
                object[] parameters = { machineName, databaseName, dwAccess };

                return (IntPtr)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("advapi32.dll",
                    "OpenSCManager", typeof(Delegates.OpenScManager), ref parameters);
            }

            public static bool QueryServiceConfig(IntPtr hService,
                out Data.Win32.Advapi32.ServiceConfig serviceConfig)
            {
                object[] parameters = { hService, IntPtr.Zero, (uint)0, (uint)0 };

                Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("advapi32.dll", "QueryServiceConfigA",
                    typeof(Delegates.QueryServiceConfig), ref parameters);

                var bytesRequired = (uint)parameters[3];
                var ptr = Marshal.AllocHGlobal((int)bytesRequired);

                parameters = new object[] { hService, ptr, bytesRequired, (uint)0 };

                var success = (bool)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("advapi32.dll",
                    "QueryServiceConfigA",
                    typeof(Delegates.QueryServiceConfig), ref parameters);

                if (!success)
                {
                    Marshal.FreeHGlobal(ptr);
                    serviceConfig = null;
                    return false;
                }

                serviceConfig = (Data.Win32.Advapi32.ServiceConfig)Marshal.PtrToStructure(
                    ptr,
                    typeof(Data.Win32.Advapi32.ServiceConfig));

                return true;
            }

            public static bool CloseServiceHandle(IntPtr hSCObject)
            {
                object[] parameters = { hSCObject };

                return (bool)Drone.Invocation.DynamicInvoke.Generic.DynamicApiInvoke("advapi32.dll",
                    "CloseServiceHandle", typeof(Delegates.CloseServiceHandle), ref parameters);
            }
        }

        private struct Delegates
        {
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool CloseHandle(IntPtr handle);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate IntPtr CreateRemoteThread(IntPtr hProcess,
                IntPtr lpThreadAttributes,
                uint dwStackSize,
                IntPtr lpStartAddress,
                IntPtr lpParameter,
                uint dwCreationFlags,
                out IntPtr lpThreadId);

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

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool IsWow64Process(
                IntPtr hProcess,
                ref bool lpSystemInfo);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate IntPtr OpenScManager(
                string machineName,
                string databaseName,
                Data.Win32.Advapi32.SCMAccess dwAccess);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool QueryServiceConfig(
                IntPtr hService,
                IntPtr lpServiceConfig,
                uint cbBufSize,
                out uint pcbBytesNeeded);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool CloseServiceHandle(IntPtr hSCObject);
        }
    }
}