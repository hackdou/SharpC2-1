namespace SharpC2.API.Response;

public class DroneResponse
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
}