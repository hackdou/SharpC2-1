using System.Runtime.Serialization;

namespace Drone.Models
{
    [DataContract]
    public class C2Message
    {
        [DataMember (Name = "direction")]
        public MessageDirection Direction { get; set; }
        
        [DataMember (Name = "type")]
        public MessageType Type { get; set; }
        
        [DataMember (Name = "metadata")]
        public Metadata Metadata { get; set; }
        
        [DataMember (Name = "data")]
        public string Data { get; set; }

        public C2Message(MessageDirection direction, MessageType type, Metadata metadata)
        {
            Direction = direction;
            Type = type;
            Metadata = metadata;
        }
        
        public enum MessageDirection : int
        {
            Upstream = 0,
            Downstream = 1
        }
        
        public enum MessageType : int
        {
            DroneModule = 0,
            DroneTask = 1,
            DroneTaskUpdate = 2
        }
    }
}