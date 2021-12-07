using System.Collections.Generic;
using System.Threading.Tasks;

using TeamServer.Models;

namespace TeamServer.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<MessageEnvelope>> GetDroneTasks(DroneMetadata metadata);
    }
}