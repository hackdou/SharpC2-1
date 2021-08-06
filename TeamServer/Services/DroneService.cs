using System.Collections.Generic;
using System.Linq;

using TeamServer.Interfaces;
using TeamServer.Models;

namespace TeamServer.Services
{
    public class DroneService : IDroneService
    {
        private readonly List<Drone> _drones = new();

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

        public bool RemoveDrone(string guid)
        {
            var drone = GetDrone(guid);
            return _drones.Remove(drone);
        }
    }
}