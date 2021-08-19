using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using SharpC2.Interfaces;
using SharpC2.Models;

namespace SharpC2.Screens
{
    public class PayloadsScreen : Screen
    {
        public IEnumerable<Handler> Handlers { get; private set; }
        
        private readonly Payload _payload = new();
        private readonly IApiService _api;

        public PayloadsScreen(IApiService api)
        {
            _api = api;
        }
        
        public override void AddCommands()
        {
            Commands.Add(new ScreenCommand("show", "Show payload options", ShowPayload));
            Commands.Add(new ScreenCommand("set", "Set a payload option", SetOption, "set <key> <value>"));
            Commands.Add(new ScreenCommand("generate", "Generate payload", GeneratePayload, "generate </output/path>"));
            
            ReadLine.AutoCompletionHandler = new PayloadsAutoComplete(this);
        }

        public override async Task LoadInitialData()
        {
            Handlers = await _api.GetHandlers();
        }

        private async Task<bool> GeneratePayload(string[] args)
        {
            if (args.Length < 2) return false;
            var path = args[1];
            
            var targetDirectory = Path.GetDirectoryName(path);
            if (!Directory.Exists(targetDirectory))
            {
                CustomConsole.WriteError("Target directory does not exist.");
                return false;
            }

            if (string.IsNullOrEmpty(_payload.Handler))
            {
                CustomConsole.WriteError("Please specify a Handler.");
                return false;
            }
            
            var payload = await _api.GeneratePayload(_payload);

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

        public Task<bool> SetOption(string[] args)
        {
            if (args.Length < 3) return Task.FromResult(false);
            
            var key = args[1];
            var value = args[2];

            if (key.Equals("handler", StringComparison.OrdinalIgnoreCase))
                _payload.Handler = value;
            
            if (key.Equals("format", StringComparison.OrdinalIgnoreCase))
                _payload.Format = FormatFromString(value);
            
            if (key.Equals("dllexport", StringComparison.OrdinalIgnoreCase))
                _payload.DllExport = value;
            
            if (key.Equals("spawnto", StringComparison.OrdinalIgnoreCase))
                _payload.SpawnTo = value;
            
            return Task.FromResult(true);
        }

        private Task<bool> ShowPayload(string[] args)
        {
            SharpSploitResultList<Payload> list = new() { _payload };
            Console.WriteLine(list.ToString());
            
            return Task.FromResult(true);
        }

        private static Payload.PayloadFormat FormatFromString(string value)
        {
            if (value.Equals("exe", StringComparison.OrdinalIgnoreCase)) return Payload.PayloadFormat.Exe;
            if (value.Equals("dll", StringComparison.OrdinalIgnoreCase)) return Payload.PayloadFormat.Dll;
            if (value.Equals("powershell", StringComparison.OrdinalIgnoreCase)) return Payload.PayloadFormat.PowerShell;
            if (value.Equals("raw", StringComparison.OrdinalIgnoreCase)) return Payload.PayloadFormat.Raw;
            if (value.Equals("svc", StringComparison.OrdinalIgnoreCase)) return Payload.PayloadFormat.Svc;

            throw new Exception("Invalid payload format");
        }
    }

    public class PayloadsAutoComplete : AutoCompleteHandler
    {
        private readonly PayloadsScreen _screen;

        public PayloadsAutoComplete(PayloadsScreen screen)
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
                if (split[0].StartsWith("help", StringComparison.OrdinalIgnoreCase))
                    return commands.Where(c => c.StartsWith(split[1])).ToArray();

                if (split[0].StartsWith("set", StringComparison.OrdinalIgnoreCase))
                    return new[] {"handler", "format", "dllexport"};
                
                if (text.StartsWith("generate", StringComparison.OrdinalIgnoreCase))
                    return Extensions.GetPartialPath(split[1]).ToArray();
            }

            if (split.Length == 3)
            {
                if (split[1].Contains("handler", StringComparison.OrdinalIgnoreCase))
                    return _screen.Handlers.Select(h => h.Name).ToArray();

                if (split[1].Contains("format", StringComparison.OrdinalIgnoreCase))
                    return Enum.GetValues(typeof(Payload.PayloadFormat))
                        .Cast<Payload.PayloadFormat>()
                        .Select(f => f.ToString())
                        .ToArray();
            }
            
            return Array.Empty<string>();
        }
    }
}