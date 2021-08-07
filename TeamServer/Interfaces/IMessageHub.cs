using System.Threading.Tasks;

namespace TeamServer.Interfaces
{
    public interface IMessageHub
    {
        Task HandlerLoaded(string handler);
        Task HandlerStarted(string handler);
        Task HandlerStopped(string handler);

        Task DroneDataSent(string droneGuid, int messageSize);
        Task DroneTasked(string droneGuid, string taskGuid);
        Task DroneTaskRunning(string droneGuid, byte[] result);
        Task DroneTaskComplete(string droneGuid, byte[] result);
        Task DroneTaskCancelled(string droneGuid, string taskGuid);
        Task DroneTaskAborted(string droneGuid, byte[] error);
    }
}