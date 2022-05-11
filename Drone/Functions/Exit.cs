using Drone.Models;

namespace Drone.Functions;

public class Exit : DroneFunction
{
    public override string Name => "exit";
    
    public override void Execute(DroneTask task)
    {
        Drone.Stop();
    }
}