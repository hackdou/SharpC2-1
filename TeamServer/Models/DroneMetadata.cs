namespace TeamServer.Models
{
    public class DroneMetadata
    {
        public string Guid { get; set; }
        public string Address { get; set; }
        public string Hostname { get; set; }
        public string Username { get; set; }
        public string Process { get; set; }
        public int Pid { get; set; }
        public string Arch { get; set; }
    }
}