using ProtoBuf;

namespace Drone.Models;

[ProtoContract]
public class Metadata
{
    [ProtoMember(1)]
    public string Id { get; set; }
    
    [ProtoMember(2)]
    public string InternalAddress { get; set; }
    
    [ProtoMember(3)]
    public string Hostname { get; set; }
    
    [ProtoMember(4)]
    public string User { get; set; }
    
    [ProtoMember(5)]
    public string Process { get; set; }
    
    [ProtoMember(6)]
    public int ProcessId { get; set; }
    
    [ProtoMember(7)]
    public string Integrity { get; set; }
    
    [ProtoMember(8)]
    public string Architecture { get; set; }
}