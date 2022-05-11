namespace TeamServer.Interfaces;

public interface IHubService
{
    // profiles
    Task NotifyProfileCreated(string name);
    Task NotifyProfileUpdated(string name);
    Task NotifyProfileDeleted(string name);

    // handlers
    Task NotifyHttpHandlerCreated(string name);
    Task NotifyHttpHandlerDeleted(string name);
    Task NotifyHttpHandlerUpdated(string name);
    Task NotifyHandlerStateChanged(string name);

    // drones
    Task NotifyNewDrone(string id);
    Task NotifyDroneCheckedIn(string id);
    Task NotifyDroneRemoved(string id);

    // tasks
    Task NotifyDroneTaskUpdated(string droneId, string taskId);
}