namespace SharpC2.API.Response;

public class DroneTaskResponse
{
    public string TaskId { get; set; }
    public string DroneId { get; set; }
    public string DroneFunction { get; set; }
    public string CommandAlias { get; set; }
    public string[] Parameters { get; set; }
    public string ArtefactPath { get; set; }
    public DateTime StartTime { get; set; }
    public int Status { get; set; }
    public DateTime EndTime { get; set; }
    public byte[] Result { get; set; }
}