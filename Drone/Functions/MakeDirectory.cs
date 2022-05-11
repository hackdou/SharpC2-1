using System.IO;
using Drone.Models;

namespace Drone.Functions;

public class MakeDirectory : DroneFunction
{
    public override string Name => "mkdir";
    
    public override void Execute(DroneTask task)
    {
        var info = Directory.CreateDirectory(task.Parameters[0]);
        Drone.SendOutput(task.TaskId, info.FullName);
    }
}