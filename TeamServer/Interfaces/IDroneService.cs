using TeamServer.Models;

namespace TeamServer.Interfaces;

public interface IDroneService
{
    // create
    Task AddDrone(Drone drone);
    
    // read
    Task<Drone> GetDrone(string id);
    Task<IEnumerable<Drone>> GetDrones();

    // update
    Task UpdateDrone(Drone drone);

    // delete
    Task DeleteDrone(Drone drone);
}