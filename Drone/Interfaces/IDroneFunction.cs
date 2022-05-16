using Drone.Models;

namespace Drone.Interfaces;

public interface IDroneFunction
{
    void Init(Drone drone);
    void Execute(DroneTask task);
}