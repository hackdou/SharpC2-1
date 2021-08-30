using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;

using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Models;

using Module = TeamServer.Modules.Module;

namespace TeamServer.Services
{
    public class ServerService : IServerService
    {
        private C2Profile _profile; 
            
        private readonly IDroneService _drones;
        private readonly IHubContext<MessageHub, IMessageHub> _hub;
        
        private readonly List<Module> _modules = new();

        public ServerService(IDroneService drones, IHubContext<MessageHub, IMessageHub> hub)
        {
            _drones = drones;
            _hub = hub;
            
            LoadDefaultModules();
        }

        public void SetC2Profile(C2Profile profile)
        {
            _profile = profile;
        }

        public C2Profile GetC2Profile()
        {
            return _profile ?? new C2Profile();
        }

        public Module LoadModule(byte[] bytes)
        {
            var asm = Assembly.Load(bytes);
            
            foreach (var module in LoadModulesFromTypes(asm.GetTypes()))
            {
                RegisterModule(module);
                return module;
            }

            return null;
        }

        public Module GetModule(string name)
        {
            return GetModules().FirstOrDefault(m =>
                m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Module> GetModules()
        {
            return _modules;
        }

        public async Task HandleC2Message(C2Message message)
        {
            var drone = _drones.GetDrone(message.Metadata.Guid);

            if (drone is null)
            {
                drone = new Drone(message.Metadata);
                _drones.AddDrone(drone);
            }
            
            drone.CheckIn();
            await _hub.Clients.All.DroneCheckedIn(drone.Metadata.Guid);

            switch (message.Type)
            {
                case C2Message.MessageType.DroneModule:
                    var module = message.Data.Deserialize<DroneModule>();
                    await HandleRegisterDroneModule(message.Metadata, module);
                    break;

                case C2Message.MessageType.DroneTaskUpdate:
                    var update = message.Data.Deserialize<DroneTaskUpdate>();
                    await HandleTaskUpdate(message.Metadata, update);
                    break;
                
                default:
                    return;
            }
        }

        private async Task HandleTaskUpdate(DroneMetadata metadata, DroneTaskUpdate update)
        {
            var module = _modules.FirstOrDefault(m =>
                m.Name.Equals(update.ServerModule, StringComparison.OrdinalIgnoreCase));

            if (module is null) return;
            await module.Execute(metadata, update);
        }

        private async Task HandleRegisterDroneModule(DroneMetadata metadata, DroneModule module)
        {
            var drone = _drones.GetDrone(metadata.Guid);
            await _hub.Clients.All.DroneModuleLoaded(metadata.Guid, module);
            drone.AddModule(module);
        }

        private void LoadDefaultModules()
        {
            var self = Assembly.GetExecutingAssembly();
            
            foreach (var module in LoadModulesFromTypes(self.GetTypes()))
            {
                RegisterModule(module);
            }
        }

        private IEnumerable<Module> LoadModulesFromTypes(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                if (!type.IsSubclassOf(typeof(Module))) continue;

                yield return Activator.CreateInstance(type) as Module;
            }
        }

        private void RegisterModule(Module module)
        {
            module.Init(_drones, _hub);
            _modules.Add(module);
        }
    }
}