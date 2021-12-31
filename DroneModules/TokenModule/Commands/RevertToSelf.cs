using System.ComponentModel;
using System.Runtime.InteropServices;

using Drone.Models;

namespace TokenModule;

public partial class TokenModule
{
    private void RevertToSelf(DroneTask task, CancellationToken token)
    {
        if (Invocation.DynamicInvoke.Win32.Advapi32.RevertToSelf())
        {
            Drone.SendResult(task.TaskGuid, "Token reverted");
        }
        else
        {
            var e = new Win32Exception(Marshal.GetLastWin32Error());
            Drone.SendError(task.TaskGuid, $"Failed to revert token. {e.Message}.");
        }
    }
}