using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SharpC2.Models;

namespace SharpC2.Screens
{
    public abstract class Screen : IDisposable
    {
        public delegate Task<bool> Callback(string[] args);
        
        public string Name { get; private set; }
        public List<ScreenCommand> Commands { get; } = new();

        public abstract void AddCommands();
        
        public virtual Task LoadInitialData() => Task.CompletedTask;
        
        private bool RunScreen { get; set; } = true;

        protected Screen()
        {
            Commands.Add(new ScreenCommand(name: "back", description: "Back to previous screen", callback: BackScreen));
            Commands.Add(new ScreenCommand(name: "help", description: "Get help", usage: "help [command]", callback: ShowHelp));
            Commands.Add(new ScreenCommand(name: "exit", description: "Exit this client", callback: ExitClient));
        }

        public void SetName(string name) => Name = name;

        public async Task Show()
        {
            while (RunScreen)
            {
                var input = ReadLine.Read($"[{Name}] # ");

                if (string.IsNullOrEmpty(input)) continue;

                var split = input.Split(' ');
                var cmd = split[0];

                var screenCommand = Commands.FirstOrDefault(c =>
                    c.Name.Equals(cmd, StringComparison.OrdinalIgnoreCase));

                if (screenCommand is not null)
                {
                    var log = await screenCommand.Callback.Invoke(split);
                    
                    if (log)
                        ReadLine.AddHistory(input);
                }
                else
                {
                    CustomConsole.WriteError("Unknown command");
                }
            }
        }

        private Task<bool> ShowHelp(string[] args)
        {
            if (args.Length == 1)
            {
                SharpSploitResultList<ScreenCommand> commands = new();
                commands.AddRange(Commands.OrderBy(c => c.Name));
                Console.WriteLine(commands.ToString());
                
                return Task.FromResult(false);
            }

            var cmd = Commands.FirstOrDefault(c =>
                c.Name.Equals(args[1], StringComparison.OrdinalIgnoreCase));

            if (cmd is null)
            {
                CustomConsole.WriteError("Unknown command");
                return Task.FromResult(false);
            }

            if (string.IsNullOrEmpty(cmd.Usage))
            {
                CustomConsole.WriteWarning("No usage documented");
                return Task.FromResult(false);
            }
            
            Console.WriteLine(cmd.Description);
            Console.WriteLine($"Usage: {cmd.Usage}");

            return Task.FromResult(false);
        }

        private Task<bool> BackScreen(string[] args)
        {
            // don't exit on this screen
            if (!Name.Equals("drones", StringComparison.OrdinalIgnoreCase))
                RunScreen = !RunScreen;
            
            return Task.FromResult(false);
        }

        private static Task<bool> ExitClient(string[] args)
        {
            Environment.Exit(0);
            return Task.FromResult(false);
        }
        
        public enum ScreenType
        {
            Drones,
            Handlers,
            HandlerConfig,
            DroneInteract
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) Commands.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}