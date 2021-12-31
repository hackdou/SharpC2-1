using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;

using Drone.SharpSploit.Generic;

namespace StandardModule.SharpSploit.Enumeration
{
    public static class Host
    {
        public static SharpSploitResultList<ProcessListResult> GetProcessListing()
        {
            var result = new SharpSploitResultList<ProcessListResult>();
            var processes = Process.GetProcesses().OrderBy(p => p.Id);
            var os64Bit = Environment.Is64BitOperatingSystem;

            foreach (var process in processes)
            {
                var pid = process.Id;
                var ppid = GetProcessParent(process);
                var name = process.ProcessName;
                var path = GetProcessPath(process);
                var session = process.SessionId;
                var owner = GetProcessOwner(process);
                var arch = !os64Bit ? "x86" : GetProcessArch(process);

                result.Add(new ProcessListResult
                {
                    Pid = pid,
                    PPid = ppid,
                    Name = name,
                    Path = path,
                    SessionId = session,
                    Owner = owner,
                    Arch = arch
                });
            }

            return result;
        }

        private static int GetProcessParent(Process process)
        {
            try
            {
                var pbi = Invocation.DynamicInvoke.Native.NtQueryInformationProcessBasicInformation(process.Handle);
                return pbi.InheritedFromUniqueProcessId;
            }
            catch
            {
                return 0;
            }
        }

        private static string GetProcessPath(Process process)
        {
            try
            {
                return process.MainModule?.FileName;
            }
            catch
            {
                return "-";
            }
        }

        private static string GetProcessOwner(Process process)
        {
            try
            {
                var hToken = IntPtr.Zero;

                if (!Invocation.DynamicInvoke.Win32.Advapi32.OpenProcessToken(process.Handle,
                    Invocation.Data.Win32.Advapi32.TokenAccess.TOKEN_ALL_ACCESS, ref hToken))
                    return "-";

                using var identity = new WindowsIdentity(hToken);

                Invocation.DynamicInvoke.Win32.Kernel32.CloseHandle(hToken);

                return identity.Name;
            }
            catch
            {
                return "-";
            }
        }

        private static string GetProcessArch(Process process)
        {
            try
            {
                var isx86 = false;
                Invocation.DynamicInvoke.Win32.Kernel32.IsWow64Process(process.Handle, ref isx86);

                return isx86 ? "x86" : "x64";
            }
            catch
            {
                return "-";
            }
        }

        public static SharpSploitResultList<ServiceResult> GetServiceListing(string computerName = null)
        {
            var results = new SharpSploitResultList<ServiceResult>();

            var services = string.IsNullOrEmpty(computerName)
                ? ServiceController.GetServices()
                : ServiceController.GetServices(computerName);

            foreach (var service in services.OrderBy(s => s.ServiceName))
            {
                try
                {
                    var config = QueryService(service.ServiceHandle.DangerousGetHandle());
                    if (config is null) continue;

                    results.Add(new ServiceResult
                    {
                        ServiceName = service.ServiceName,
                        DisplayName = service.DisplayName,
                        StartType = config.dwStartType,
                        BinaryPath = config.lpBinaryPathName,
                        Status = service.Status
                    });
                }
                catch
                {
                    // may get an access denied, in which case, just add the name
                    results.Add(new ServiceResult
                    {
                        ServiceName = service.ServiceName,
                        DisplayName = service.DisplayName
                    });
                }
            }

            return results;
        }

        private static Invocation.Data.Win32.Advapi32.ServiceConfig QueryService(IntPtr hService)
        {
            try
            {
                _ = Invocation.DynamicInvoke.Win32.Advapi32.QueryServiceConfig(hService, out var serviceConfig);
                return serviceConfig;
            }
            finally
            {
                Invocation.DynamicInvoke.Win32.Advapi32.CloseServiceHandle(hService);
            }
        }
    }

    public sealed class ProcessListResult : SharpSploitResult
    {
        public int Pid { get; set; }
        public int PPid { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public int SessionId { get; set; }
        public string Owner { get; set; }
        public string Arch { get; set; }

        public override IList<SharpSploitResultProperty> ResultProperties =>
            new List<SharpSploitResultProperty>
            {
                new() { Name = "PID", Value = Pid },
                new() { Name = "PPID", Value = PPid },
                new() { Name = "Name", Value = Name },
                new() { Name = "Path", Value = Path },
                new() { Name = "SessionId", Value = SessionId },
                new() { Name = "Owner", Value = Owner },
                new() { Name = "Arch", Value = Arch }
            };
    }

    public sealed class ServiceResult : SharpSploitResult
    {
        public string ServiceName { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public Invocation.Data.Win32.Advapi32.ServiceStartType StartType { get; set; }
        public string BinaryPath { get; set; } = "-";
        public ServiceControllerStatus Status { get; set; }
        
        public override IList<SharpSploitResultProperty> ResultProperties =>
            new List<SharpSploitResultProperty>
            {
                new() { Name = "ServiceName", Value = ServiceName },
                new() { Name = "DisplayName", Value = DisplayName },
                new() { Name = "StartType", Value = StartType },
                new() { Name = "BinaryPath", Value = BinaryPath },
                new() { Name = "Status", Value = Status }
            };
    }
}