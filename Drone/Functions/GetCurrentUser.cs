using System.Security.Principal;

using Drone.Models;

namespace Drone.Functions;

public class GetCurrentUser : DroneFunction
{
    public override string Name => "getuid";
    
    public override void Execute(DroneTask task)
    {
        using var identity = WindowsIdentity.GetCurrent();
        Drone.SendOutput(task.TaskId, identity.Name);
    }
}