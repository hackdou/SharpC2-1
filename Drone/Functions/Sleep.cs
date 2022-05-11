using Drone.Models;

namespace Drone.Functions;

public class Sleep : DroneFunction
{
    public override string Name => "sleep";
    
    public override void Execute(DroneTask task)
    {
        if (task.Parameters.Length > 0)
            Drone.Config.Set("SleepInterval", int.Parse(task.Parameters[0]));
        
        if (task.Parameters.Length > 1)
            Drone.Config.Set("SleepJitter", int.Parse(task.Parameters[1]));
        
        Drone.SendTaskComplete(task.TaskId);
    }
}