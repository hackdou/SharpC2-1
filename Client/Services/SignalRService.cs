using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;
using SharpC2.Models;

namespace SharpC2.Services
{
    public class SignalRService
    {
        private readonly CertificateService _certs;

        public SignalRService(CertificateService certs)
        {
            _certs = certs;
        }
        
        // Handlers
        public event Action<string> HandlerLoaded;
        public event Action<string> HandlerStarted;
        public event Action<string> HandlerStopped;
        
        //Drones
        public event Action<string> DroneCheckedIn;
        public event Action<string, DroneModule> DroneModuleLoaded;
        public event Action<string, string> DroneTasked;
        public event Action<string, int> DroneDataSent;
        public event Action<string, byte[]> DroneTaskRunning;
        public event Action<string, byte[]> DroneTaskComplete;
        public event Action<string, string> DroneTaskCancelled;
        public event Action<string, byte[]> DroneTaskAborted;

        public async Task Connect(string hostname, string port, string nick, string pass)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl($"https://{hostname}:{port}/MessageHub", o =>
                {
                    var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{nick}:{pass}"));
                    o.Headers.Add("Authorization", $"Basic {basic}");
                    o.HttpMessageHandlerFactory = handler => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = _certs.RemoteCertificateValidationCallback
                    };
                })
                .Build();

            
            await connection.StartAsync();

            connection.On<string>("HandlerLoaded", msg => HandlerLoaded?.Invoke(msg));
            connection.On<string>("HandlerStarted", msg => HandlerStarted?.Invoke(msg));
            connection.On<string>("HandlerStopped", msg => HandlerStopped?.Invoke(msg));

            connection.On<string>("DroneCheckedIn", (drone) => DroneCheckedIn?.Invoke(drone));
            connection.On<string, DroneModule>("DroneModuleLoaded", (drone, module) => DroneModuleLoaded?.Invoke(drone, module));
            connection.On<string, string>("DroneTasked", (drone, task) => DroneTasked?.Invoke(drone, task));
            connection.On<string, int>("DroneDataSent", (drone, size) => DroneDataSent?.Invoke(drone, size));
            connection.On<string, byte[]>("DroneTaskRunning", (drone, result) => DroneTaskRunning?.Invoke(drone, result));
            connection.On<string, byte[]>("DroneTaskComplete", (drone, result) => DroneTaskComplete?.Invoke(drone, result));
            connection.On<string, string>("DroneTaskCancelled", (drone, task) => DroneTaskCancelled?.Invoke(drone, task));
            connection.On<string, byte[]>("DroneTaskAborted", (drone, error) => DroneTaskAborted?.Invoke(drone, error));
        }
    }
}