namespace SharpC2.API.V1.Requests
{
    public class DroneTaskRequest
    {
        public string Module { get; set; }
        public string Command { get; set; }
        public string[] Arguments { get; set; }
        public byte[] Artefact { get; set; }
    }
}