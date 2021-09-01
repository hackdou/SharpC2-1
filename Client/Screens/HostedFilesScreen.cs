using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using SharpC2.Interfaces;
using SharpC2.Models;
using SharpC2.Services;

namespace SharpC2.Screens
{
    public class HostedFilesScreen : Screen
    {
        private readonly IApiService _api;
        private readonly SignalRService _signalR;

        public HostedFilesScreen(IApiService api, SignalRService signalR)
        {
            _api = api;
            _signalR = signalR;
            
            _signalR.HostedFileAdded += OnHostedFileAdded;
            _signalR.HostedFileDeleted += OnHostedFileDeleted;
        }

        public override void AddCommands()
        {
            Commands.Add(new ScreenCommand("list", "List hosted files", ListFiles));
            Commands.Add(new ScreenCommand("add", "Add a hosted file", AddFile, "add </path/to/file> <filename>"));
            Commands.Add(new ScreenCommand("delete", "Remove a hosted file", DeleteFile, "delete <filename>"));
            
            ReadLine.AutoCompletionHandler = new HostedFilesAutoComplete(this);
        }

        private async Task<bool> ListFiles(string[] args)
        {
            var files = await _api.GetHostedFiles();
            if (files is null) return false;
            
            SharpSploitResultList<HostedFile> list = new();
            list.AddRange(files);
            
            Console.WriteLine(list.ToString());
            return true;
        }
        
        private async Task<bool> AddFile(string[] args)
        {
            if (args.Length < 2)
            {
                CustomConsole.WriteError("Not enough arguments");
                return false;
            }
            
            var path = args[1];
            var filename = args[2];
            
            if (!File.Exists(path))
            {
                CustomConsole.WriteError($"{path} does not exist.");
                return false;
            }

            var content = await File.ReadAllBytesAsync(path);

            await _api.AddHostedFile(content, filename);
            return true;
        }
        
        private async Task<bool> DeleteFile(string[] args)
        {
            if (args.Length < 1)
            {
                CustomConsole.WriteError("Not enough arguments");
                return false;
            }
            
            var filename = args[1];
            await _api.DeleteHostedFile(filename);
            return true;
        }
        
        private void OnHostedFileAdded(string filename)
            => CustomConsole.WriteMessage($"{filename} uploaded.");
        
        private void OnHostedFileDeleted(string filename)
            => CustomConsole.WriteMessage($"{filename} deleted.");

        protected override void Dispose(bool disposing)
        {
            _signalR.HostedFileAdded -= OnHostedFileAdded;
            _signalR.HostedFileDeleted -= OnHostedFileDeleted;
            
            base.Dispose(disposing);
        }
    }

    public class HostedFilesAutoComplete : AutoCompleteHandler
    {
        private readonly HostedFilesScreen _screen;

        public HostedFilesAutoComplete(HostedFilesScreen screen)
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
                if (text.StartsWith("add", StringComparison.OrdinalIgnoreCase))
                    return Extensions.GetPartialPath(split[1]).ToArray();
            }

            return Array.Empty<string>();
        }
    }
}