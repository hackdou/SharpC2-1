using System.Collections.Generic;

using TeamServer.Models;

namespace TeamServer.Interfaces
{
    public interface IDroneService
    {
        void AddDrone(Drone drone);
        IEnumerable<Drone> GetDrones();
        Drone GetDrone(string guid);
        bool RemoveDrone(string guid);
    }
}