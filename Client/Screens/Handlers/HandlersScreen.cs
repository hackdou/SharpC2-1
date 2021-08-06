using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using SharpC2.Interfaces;
using SharpC2.Models;
using SharpC2.Services;

namespace SharpC2.Screens
{
    public class HandlersScreen : Screen
    {
        private readonly IApiService _api;
        private readonly IScreenFactory _screens;
        private readonly SignalRService _signalR;

        public HandlersScreen(IApiService api, IScreenFactory screens, SignalRService signalR)
        {
            _api = api;
            _screens = screens;
            _signalR = signalR;

            _signalR.HandlerStarted += h => CustomConsole.WriteMessage($"Handler \"{h}\" started.");
            _signalR.HandlerStopped += h => CustomConsole.WriteMessage($"Handler \"{h}\" stopped.");
        }

        public IEnumerable<Handler> Handlers { get; private set; }

        public override void AddCommands()
        {
            Commands.Add(new ScreenCommand(name: "list", description: "List Handlers", callback: ListHandlers));
            Commands.Add(new ScreenCommand(name: "config", description: "Configure the given Handler", usage: "config <handler>", callback: ConfigHandler));
            Commands.Add(new ScreenCommand(name: "start", description: "Start the given Handler", usage: "start <handler>", callback: StartHandler));
            Commands.Add(new ScreenCommand(name: "stop", description: "Stop the given Handler", usage: "stop <handler>", callback: StopHandler));
            Commands.Add(new ScreenCommand(name: "payload", description: "Generate a payload for the given Handler", usage: "payload <handler> <format> <path>", callback: GeneratePayload));
            
            ReadLine.AutoCompletionHandler = new HandlersAutoComplete(this);
        }

        public override async Task LoadInitialData()
        {
            Handlers = (await _api.GetHandlers()).ToArray();
        }

        private async Task<bool> ListHandlers(string[] args)
        {
            Handlers = (await _api.GetHandlers()).ToArray();

            SharpSploitResultList<Handler> handlers = new();
            handlers.AddRange(Handlers);
            Console.WriteLine(handlers.ToString());

            return true;
        }
        
        private async Task<bool> ConfigHandler(string[] args)
        {
            var handler = args[1];
            using var screen = _screens.GetScreen(ScreenType.HandlerConfig);
            screen.SetName(handler);
            await screen.LoadInitialData();
            screen.AddCommands();
            await screen.Show();
            
            // reset autocomplete here
            ReadLine.AutoCompletionHandler = new HandlersAutoComplete(this);

            return true;
        }
        
        private async Task<bool> StartHandler(string[] args)
        {
            var handler = args[1];
            await _api.StartHandler(handler);
            
            return true;
        }
        
        private async Task<bool> StopHandler(string[] args)
        {
            var handler = args[1];
            await _api.StopHandler(handler);
            
            return true;
        }

        private async Task<bool> GeneratePayload(string[] args)
        {
            if (args.Length < 4) return false;
            
            var handler = args[1];
            var format = args[2];
            var path = args[3];

            var targetDirectory = Path.GetDirectoryName(path);
            if (!Directory.Exists(targetDirectory))
            {
                CustomConsole.WriteError("Target directory does not exist.");
                return false;
            }

            // this is pretty stupid, the API should tell us which formats are available
            if (!format.Equals("exe", StringComparison.OrdinalIgnoreCase) &&
                !format.Equals("dll", StringComparison.OrdinalIgnoreCase))
            {
                CustomConsole.WriteError("Format should be \"exe\" or \"dll\".");
                return false;
            }

            var payload = await _api.GeneratePayload(handler, format);

            try
            {
                await File.WriteAllBytesAsync(path, payload);
            }
            catch (Exception e)
            {
                CustomConsole.WriteError(e.Message);
                return false;
            }
            
            CustomConsole.WriteMessage($"Saved {payload.Length} bytes.");

            return true;
        }
    }

    public class HandlersAutoComplete : AutoCompleteHandler
    {
        private readonly HandlersScreen _screen;

        public HandlersAutoComplete(HandlersScreen screen)
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

            if (split.Length == 2)
            {
                return split[0].StartsWith("help")
                    ? commands
                    : _screen.Handlers.Select(h => h.Name).ToArray();
            }

            if (split.Length == 3)
                return new[] { "exe", "dll" };

            if (split.Length == 4)
                return Extensions.GetPartialPath(split[3]).ToArray();

            return Array.Empty<string>();
        }
    }
}