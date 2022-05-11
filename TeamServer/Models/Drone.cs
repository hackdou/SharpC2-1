namespace TeamServer.Models;

public class Drone
{
    public string Id { get; set; }
    public string ExternalAddress { get; set; }
    public string InternalAddress { get; set; }
    public string Handler { get; set; }
    public string User { get; set; }
    public string Hostname { get; set; }
    public string Process { get; set; }
    public int ProcessId { get; set; }
    public string Integrity { get; set; }
    public string Architecture { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }

    public Drone(Metadata metadata)
    {
        Id = metadata.Id;
        InternalAddress = metadata.InternalAddress;
        User = metadata.User;
        Hostname = metadata.Hostname;
        Process = metadata.Process;
        ProcessId = metadata.ProcessId;
        Integrity = metadata.Integrity;
        Architecture = metadata.Architecture;
        FirstSeen = LastSeen = DateTime.UtcNow;
    }

    public Drone()
    {
        // automapper
    }

    public void CheckIn()
    {
        LastSeen = DateTime.UtcNow;
    }
}