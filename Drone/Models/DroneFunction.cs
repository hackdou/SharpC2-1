namespace Drone.Models;

public abstract class DroneFunction
{
    protected Drone Drone { get; private set; }
    
    public abstract string Name { get; }

    public void Init(Drone drone)
    {
        Drone = drone;
    }

    public abstract void Execute(DroneTask task);
}