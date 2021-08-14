using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using SharpC2.Interfaces;
using SharpC2.Models;
using SharpC2.Services;

namespace SharpC2.Screens
{
    public class DroneInteractScreen : Screen
    {
        private readonly IApiService _api;
        private readonly SignalRService _signalR;

        public Drone Drone { get; private set; }

        public DroneInteractScreen(IApiService api, SignalRService signalR)
        {
            _api = api;
            _signalR = signalR;

            _signalR.DroneModuleLoaded += OnDroneModuleLoaded;
            _signalR.DroneTasked += OnDroneTasked;
            _signalR.DroneDataSent += OnDroneDataSent;
            _signalR.DroneTaskRunning += OnDroneTaskRunning;
            _signalR.DroneTaskComplete += OnDroneTaskComplete;
            _signalR.DroneTaskAborted += OnDroneTaskAborted;
            _signalR.DroneTaskCancelled += OnDroneTaskCancelled;
        }

        public override void AddCommands()
        {
            if (Drone is null) return;
            
            // remove default "exit" command
            var exit = Commands.FirstOrDefault(c => c.Name.Equals("exit", StringComparison.OrdinalIgnoreCase));
            Commands.Remove(exit);

            foreach (var module in Drone.Modules)
            {
                foreach (var command in module.Commands)
                {
                    Commands.Add(new ScreenCommand(
                        name: command.Name,
                        description: command.Description,
                        usage: command.Usage,
                        callback: ExecuteDroneCommand));
                }
            }
            
            ReadLine.AutoCompletionHandler = new DroneInteractAutoComplete(this);
        }

        private async Task<bool> ExecuteDroneCommand(string[] args)
        {
            // get the module from alias
            var module = Drone.Modules.FirstOrDefault(m =>
                m.Commands.Any(c =>
                    c.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase)));

            if (module is null)
            {
                CustomConsole.WriteError("Module not found");
                return false;
            }

            var command = module.Commands.FirstOrDefault(c =>
                c.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase));
            
            if (command is null)
            {
                CustomConsole.WriteError("Unknown command");
                return false;
            }
            
            // drop the alias
            args = args[1..].ToArray();

            var artefact = Array.Empty<byte>();

            // process all the args
            var commandArgs = command.Arguments?.ToArray();
            
            // first check the count to see if there are enough
            if (args.Length < commandArgs?.Where(a => !a.Optional).Count())
            {
                CustomConsole.WriteError("Not enough arguments");
                return false;
            }

            string filePath = null;
            
            if (args.Length > 0)
            {
                for (var i = 0; i < commandArgs?.Where(a => !a.Optional).Count(); i++)
                {
                    // is the arg none-optional
                    if (commandArgs[i].Optional && string.IsNullOrEmpty(args[i]))
                    {
                        CustomConsole.WriteError($"{commandArgs[i].Label} is mandatory.");
                        return false;
                    }

                    // is the arg a file
                    if (!commandArgs[i].Artefact) continue;

                    filePath = args[i];
                    if (!File.Exists(filePath))
                    {
                        CustomConsole.WriteError($"{filePath} not found.");
                        return false;
                    }

                    var extension = Path.GetExtension(filePath);
                    artefact = extension.Equals(".ps1")
                        ? Encoding.UTF8.GetBytes(await File.ReadAllTextAsync(filePath))
                        : await File.ReadAllBytesAsync(filePath);

                    
                }    
            }
            
            // remove filepath from args
            if (!string.IsNullOrEmpty(filePath))
                args = args.Where(s => !s.Equals(filePath)).ToArray();

            await _api.TaskDrone(Name, module.Name, command.Name, args, artefact);
            return true;
        }
        
        private void OnDroneModuleLoaded(string droneGuid, DroneModule module)
        {
            if (!droneGuid.Equals(Name, StringComparison.OrdinalIgnoreCase)) return;
            
            Drone.Modules.Add(module);
            
            foreach (var command in module.Commands)
            {
                Commands.Add(new ScreenCommand(
                    name: command.Name,
                    description: command.Description,
                    usage: command.Usage,
                    callback: ExecuteDroneCommand));
            }
        }

        private void OnDroneTasked(string droneGuid, string taskGuid)
        {
            if (!droneGuid.Equals(Name, StringComparison.OrdinalIgnoreCase)) return;
            CustomConsole.WriteMessage($"Drone tasked: {taskGuid}");
        }
        
        private void OnDroneDataSent(string droneGuid, int size)
        {
            if (!droneGuid.Equals(Name, StringComparison.OrdinalIgnoreCase)) return;
            CustomConsole.WriteMessage($"Drone checked in. Sent {size} bytes.");
        }

        private void OnDroneTaskRunning(string droneGuid, byte[] output)
        {
            if (!droneGuid.Equals(Name, StringComparison.OrdinalIgnoreCase)) return;
            if (output is null || output.Length == 0) return;

            var text = Encoding.UTF8.GetString(output);
            CustomConsole.WriteMessage("Output received:");
            Console.WriteLine(text);
        }
        
        private void OnDroneTaskComplete(string droneGuid, byte[] output)
        {
            if (!droneGuid.Equals(Name, StringComparison.OrdinalIgnoreCase)) return;

            if (output is not null && output.Length > 0)
            {
                var text = Encoding.UTF8.GetString(output);
                CustomConsole.WriteMessage("Output received:");
                Console.WriteLine(text);
            }

            CustomConsole.WriteMessage("Task complete.");
        }
        
        private void OnDroneTaskAborted(string droneGuid, byte[] error)
        {
            if (!droneGuid.Equals(Name, StringComparison.OrdinalIgnoreCase)) return;
            CustomConsole.WriteWarning("Task threw an exception.");

            if (error is null || error.Length <= 0) return;
            
            var text = Encoding.UTF8.GetString(error);
            CustomConsole.WriteWarning(text);
        }
        
        private void OnDroneTaskCancelled(string droneGuid, string taskGuid)
        {
            if (!droneGuid.Equals(Name, StringComparison.OrdinalIgnoreCase)) return;
            CustomConsole.WriteWarning($"Task {taskGuid} cancelled");
        }

        protected override void Dispose(bool disposing)
        {
            _signalR.DroneTasked -= OnDroneTasked;
            _signalR.DroneDataSent -= OnDroneDataSent;
            _signalR.DroneTaskRunning -= OnDroneTaskRunning;
            _signalR.DroneTaskComplete -= OnDroneTaskComplete;
        }

        public override async Task LoadInitialData()
        {
            if (string.IsNullOrEmpty(Name)) return;
            Drone = await _api.GetDrone(Name);
        }
    }
    
    public class DroneInteractAutoComplete : AutoCompleteHandler
    {
        private readonly DroneInteractScreen _screen;

        public DroneInteractAutoComplete(DroneInteractScreen screen)
        {
            _screen = screen;
        }

        public override string[] GetSuggestions(string text, int index)
        {
            var commands = _screen.Commands.Select(c => c.Name).ToArray();
            var split = text.Split(' ');

            if (split.Length == 1)
            {
                return string.IsNullOrEmpty(split[0])
                    ? commands
                    : commands.Where(c => c.StartsWith(split[0])).ToArray();
            }

            List<DroneModule.Command> droneCommands = new();
            foreach (var droneModule in _screen.Drone.Modules)
                droneCommands.AddRange(droneModule.Commands);

            var matchedCommand = droneCommands.FirstOrDefault(c => c.Name.Equals(split[0], StringComparison.OrdinalIgnoreCase));
            if (matchedCommand is null) return split[0].StartsWith("help") ? commands : Array.Empty<string>();

            if (matchedCommand.Arguments.Length >= split.Length - 1)
            {
                var arg = matchedCommand.Arguments[split.Length - 2];
                if (arg.Artefact) return Extensions.GetPartialPath(split[1]).ToArray();
            }

            return Array.Empty<string>();
        }
    }
}