namespace TeamServer.Models;

public class DroneTaskRecord
{
    public string TaskId { get; set; }
    public string DroneId { get; set; }
    public string DroneFunction { get; set; }
    public string CommandAlias { get; set; }
    public string[] Parameters { get; set; }
    public string ArtefactPath { get; set; }
    public byte[] Artefact { get; set; }
    public DateTime StartTime { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime EndTime { get; set; }
    public byte[] Result { get; set; }
    
    public enum TaskStatus : int
    {
        Pending = 0,
        Tasked = 1,
        Running = 2,
        Complete = 3,
        Aborted = 4
    }
}