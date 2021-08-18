using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TeamServer.Interfaces;
using TeamServer.Models;

namespace TeamServer.Services
{
    public class TaskService : ITaskService
    {
        private readonly IServerService _server;
        private readonly IDroneService _drones;

        public TaskService(IServerService server, IDroneService drones)
        {
            _server = server;
            _drones = drones;
        }

        public async Task RecvC2Data(IEnumerable<C2Message> messages)
        {
            foreach (var message in messages)
                await _server.HandleC2Message(message);
        }

        public async Task<C2Message> GetDroneTasks(DroneMetadata metadata)
        {
            var drone = _drones.GetDrone(metadata.Guid);

            if (drone is null)
            {
                drone = new Drone(metadata);
                
                // new drone, send stdapi.dll
                drone.TaskDrone(new DroneTask("core", "load-module")
                {
                    Artefact = await Utilities.GetEmbeddedResource("stdapi.dll")
                });
                
                _drones.AddDrone(drone);
            }
                
            drone.CheckIn();

            var tasks = drone.GetPendingTasks().ToArray();
            if (!tasks.Any()) return null;

            return new C2Message(C2Message.MessageDirection.Downstream, C2Message.MessageType.DroneTask)
            {
                Data = tasks.Serialize()
            };
        }
    }
}