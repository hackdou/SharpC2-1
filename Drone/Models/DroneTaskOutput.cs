using ProtoBuf;

namespace Drone.Models;

[ProtoContract]
public class DroneTaskOutput
{
    [ProtoMember(1)]
    public string TaskId { get; set; }
    
    [ProtoMember(2)]
    public TaskStatus Status { get; set; }
    
    [ProtoMember(3)]
    public byte[] Output { get; set; }
    
    public enum TaskStatus : int
    {
        Running = 2,
        Complete = 3,
        Aborted = 4
    }
}