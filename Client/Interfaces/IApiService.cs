using System.Collections.Generic;
using System.Threading.Tasks;

using SharpC2.Models;

namespace SharpC2.Interfaces
{
    public interface IApiService
    {
        void InitClient(string hostname, string port, string nick, string pass);
        
        // Handlers
        Task<IEnumerable<Handler>> GetHandlers();
        Task<Handler> GetHandler(string name);
        Task LoadHandler(byte[] handler);
        Task SetHandlerParameter(string name, string key, string value);
        Task StartHandler(string name);
        Task StopHandler(string name);
        Task<byte[]> GeneratePayload(string name, string format);
        
        // Drones
        Task<IEnumerable<Drone>> GetDrones();
        Task<Drone> GetDrone(string guid);
        Task TaskDrone(string guid, string module, string command, string[] args, byte[] artefact);
        Task<IEnumerable<DroneTask>> GetDroneTasks(string droneGuid);
        Task<DroneTask> GetDroneTask(string droneGuid, string taskGuid);
    }
}