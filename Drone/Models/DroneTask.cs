using System;

using ProtoBuf;

namespace Drone.Models;

[ProtoContract]
public class DroneTask
{
    [ProtoMember(1)]
    public string TaskId { get; set; }
    
    [ProtoMember(2)]
    public string Function { get; set; }

    [ProtoMember(3)]
    public string[] Parameters { get; set; } = Array.Empty<string>();

    [ProtoMember(4)]
    public byte[] Artefact { get; set; } = Array.Empty<byte>();
}