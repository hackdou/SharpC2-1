using System.Collections.Generic;
using System.Threading.Tasks;

using TeamServer.Models;

namespace TeamServer.Interfaces
{
    public interface ITaskService
    {
        Task RecvC2Data(IEnumerable<C2Message> messages);
        Task<C2Message> GetDroneTasks(DroneMetadata metadata);
    }
}