using TeamServer.Models;

namespace TeamServer.Interfaces;

public interface ITaskService
{
    // create
    Task AddTask(DroneTaskRecord task);
    
    // read
    Task<DroneTaskRecord> GetTask(string taskId);
    Task<IEnumerable<DroneTaskRecord>> GetAllTasks();
    Task<IEnumerable<DroneTaskRecord>> GetTasks(string droneId);
    
    Task<IEnumerable<DroneTask>> GetPendingTasks(string droneId);

    // update
    Task UpdateTasks(IEnumerable<DroneTaskOutput> outputs);

    // no deletes
}