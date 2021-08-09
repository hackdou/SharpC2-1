using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SharpC2.Interfaces;
using SharpC2.Models;
using SharpC2.Services;

namespace SharpC2.Screens
{
    public class DroneScreen : Screen
    {
        private readonly IApiService _api;
        private readonly IScreenFactory _screens;
        private readonly SignalRService _signalR;
        
        public List<Drone> Drones { get; } = new();

        public DroneScreen(IApiService api, IScreenFactory screens, SignalRService signalR)
        {
            _api = api;
            _screens = screens;
            _signalR = signalR;
        }

        public override void AddCommands()
        {
            Commands.Add(new ScreenCommand(name: "list", description: "List Drones", callback: ListDrones));
            Commands.Add(new ScreenCommand(name: "handlers", description: "Go to Handlers", callback: OpenHandlerScreen));
            Commands.Add(new ScreenCommand(name: "payloads", description: "Go to Payloads", callback: OpenPayloadsScreen));
            Commands.Add(new ScreenCommand(name: "interact", description: "Interact with the given Drone", usage: "interact <drone>", callback: DroneInteract));
            
            ReadLine.AutoCompletionHandler = new DronesAutoComplete(this);
        }

        private async Task<bool> ListDrones(string[] args)
        {
            var drones = await _api.GetDrones();

            foreach (var drone in drones)
            {
                var existing = Drones.FirstOrDefault(d => d.Guid.Equals(drone.Guid, StringComparison.OrdinalIgnoreCase));
                if (existing is not null) Drones.Remove(existing);
                
                Drones.Add(drone);
            }

            SharpSploitResultList<Drone> list = new();
            list.AddRange(Drones);
            Console.WriteLine(list.ToString());

            return true;
        }

        private async Task<bool> OpenHandlerScreen(string[] args)
        {
            using var screen = _screens.GetScreen(ScreenType.Handlers);
            screen.SetName("handlers");
            await screen.LoadInitialData();
            screen.AddCommands();
            await screen.Show();
            
            // reset autocomplete
            ReadLine.AutoCompletionHandler = new DronesAutoComplete(this);

            return true;
        }
        
        private async Task<bool> OpenPayloadsScreen(string[] args)
        {
            using var screen = _screens.GetScreen(ScreenType.Payloads);
            screen.SetName("payloads");
            screen.AddCommands();
            await screen.LoadInitialData();
            await screen.Show();
            
            // reset autocomplete
            ReadLine.AutoCompletionHandler = new DronesAutoComplete(this);

            return true;
        }
        
        private async Task<bool> DroneInteract(string[] args)
        {
            if (args.Length < 2) return false;
            var drone = args[1];
            if (string.IsNullOrEmpty(drone)) return false;
            
            using var screen = _screens.GetScreen(ScreenType.DroneInteract);
            screen.SetName(drone);
            await screen.LoadInitialData();
            screen.AddCommands();
            await screen.Show();
            
            // reset autocomplete
            ReadLine.AutoCompletionHandler = new DronesAutoComplete(this);

            return true;
        }
    }
    
    public class DronesAutoComplete : AutoCompleteHandler
    {
        private readonly DroneScreen _screen;

        public DronesAutoComplete(DroneScreen screen)
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
                return _screen.Drones.Select(d => d.Guid).ToArray();

            return Array.Empty<string>();
        }
    }
}