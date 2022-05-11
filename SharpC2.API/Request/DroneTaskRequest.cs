namespace SharpC2.API.Request;

public class DroneTaskRequest
{
    public string DroneId { get; set; }
    public string DroneFunction { get; set; }
    public string CommandAlias { get; set; }
    public string[] Parameters { get; set; }
    public string ArtefactPath { get; set; }
    public byte[] Artefact { get; set; }
}