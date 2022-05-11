using System.IO;
using Drone.Models;

namespace Drone.Functions;

public class RemoveDirectory : DroneFunction
{
    public override string Name => "rmdir";
    
    public override void Execute(DroneTask task)
    {
        Directory.Delete(task.Parameters[0], true);
        Drone.SendTaskComplete(task.TaskId);
    }
}