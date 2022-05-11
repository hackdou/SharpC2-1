using System.IO;
using Drone.Models;

namespace Drone.Functions;

public class RemoveFile : DroneFunction
{
    public override string Name => "rm";
    
    public override void Execute(DroneTask task)
    {
        File.Delete(task.Parameters[0]);
        Drone.SendTaskComplete(task.TaskId);
    }
}