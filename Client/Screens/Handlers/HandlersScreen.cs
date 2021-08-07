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

            _signalR.HandlerLoaded += OnHandlerLoaded;
            _signalR.HandlerStarted += OnHandlerStarted;
            _signalR.HandlerStopped += OnHandlerStopped;
        }

        public IEnumerable<Handler> Handlers { get; private set; }

        public override void AddCommands()
        {
            Commands.Add(new ScreenCommand("load", "Load a Handler DLL", LoadHandler, "load </path/to/handler.dll"));
            Commands.Add(new ScreenCommand("list", "List Handlers", ListHandlers));
            Commands.Add(new ScreenCommand("config", "Configure the given Handler", ConfigHandler, "config <handler>"));
            Commands.Add(new ScreenCommand("start", "Start the given Handler", StartHandler, "start <handler>"));
            Commands.Add(new ScreenCommand("stop", "Stop the given Handler", StopHandler, "stop <handler>"));
            Commands.Add(new ScreenCommand("payload", "Generate a payload for the given Handler", GeneratePayload, "payload <handler> <format> <path>"));
            
            ReadLine.AutoCompletionHandler = new HandlersAutoComplete(this);
        }

        public override async Task LoadInitialData()
        {
            Handlers = (await _api.GetHandlers()).ToArray();
        }
        
        private void OnHandlerLoaded(string handler)
            => CustomConsole.WriteMessage($"Handler \"{handler}\" loaded");

        private void OnHandlerStarted(string handler)
            => CustomConsole.WriteMessage($"Handler \"{handler}\" started.");

        private void OnHandlerStopped(string handler)
            => CustomConsole.WriteMessage($"Handler \"{handler}\" stopped.");

        private async Task<bool> LoadHandler(string[] args)
        {
            if (args.Length < 1) return false;
            var path = args[1];
            
            if (!File.Exists(path))
            {
                CustomConsole.WriteError($"{path} not found");
                return false;
            }

            var bytes = await File.ReadAllBytesAsync(path);
            await _api.LoadHandler(bytes);
            return true;
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

        protected override void Dispose(bool disposing)
        {
            _signalR.HandlerLoaded -= OnHandlerLoaded;
            _signalR.HandlerStarted -= OnHandlerStarted;
            _signalR.HandlerStopped -= OnHandlerStopped;
            
            base.Dispose(disposing);
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
                if (split[0].StartsWith("help"))
                    return _screen.Handlers.Select(h => h.Name).ToArray();

                if (split[0].StartsWith("load"))
                    return Extensions.GetPartialPath(split[1]).ToArray();
            }

            if (split.Length == 3)
                return new[] { "exe", "dll" };

            if (split.Length == 4)
                return Extensions.GetPartialPath(split[3]).ToArray();

            return Array.Empty<string>();
        }
    }
}