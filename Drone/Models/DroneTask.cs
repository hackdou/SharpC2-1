using System.Runtime.Serialization;

namespace Drone.Models
{
    [DataContract]
    public class DroneTask
    {
        [DataMember (Name = "TaskGuid")]
        public string TaskGuid { get; set; }
        
        [DataMember (Name = "Module")]
        public string Module { get; set; }
        
        [DataMember (Name = "Command")]
        public string Command { get; set; }
        
        [DataMember (Name = "Arguments")]
        public string[] Arguments { get; set; }
        
        [DataMember (Name = "Artefact")]
        public string Artefact { get; set; }
    }
}