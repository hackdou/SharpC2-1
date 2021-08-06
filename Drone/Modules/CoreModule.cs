using System;
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

            Commands.Add(sleep);
            Commands.Add(amsi);
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
        
        private void ExitDrone(DroneTask task, CancellationToken token)
        {
            Drone.Stop();
        }
    }
}