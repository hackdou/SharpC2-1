using System;

using SharpC2.Interfaces;
using SharpC2.Services;

namespace SharpC2.Screens
{
    public class ScreenFactory : IScreenFactory
    {
        private readonly IApiService _apiService;
        private readonly SignalRService _signalR;

        public ScreenFactory(IApiService apiService, SignalRService signalR)
        {
            _apiService = apiService;
            _signalR = signalR;
        }

        public Screen GetScreen(Screen.ScreenType type)
        {
            Screen screen = type switch
            {
                Screen.ScreenType.Drones => new DroneScreen(_apiService, this, _signalR),
                Screen.ScreenType.Handlers => new HandlersScreen(_apiService, this, _signalR),
                Screen.ScreenType.HandlerConfig => new ConfigHandlerScreen(_apiService),
                Screen.ScreenType.DroneInteract => new DroneInteractScreen(_apiService, _signalR),
                Screen.ScreenType.Payloads => new PayloadsScreen(_apiService),
                Screen.ScreenType.HostedFiles => new HostedFilesScreen(_apiService, _signalR),
                
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            return screen;
        }
    }
}