using System.IO;

using Drone.Models;

namespace Drone.Functions;

public class PrintDirectory : DroneFunction
{
    public override string Name => "pwd";
    
    public override void Execute(DroneTask task)
    {
        Drone.SendOutput(task.TaskId,
            Directory.GetCurrentDirectory());
    }
}