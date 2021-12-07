using System.Runtime.Serialization;

namespace Drone.Models
{
    [DataContract]
    public class PivotHandler
    {
        [DataMember]
        public string HandlerName { get; set; }
        
        [DataMember]
        public string Hostname { get; set; }
        
        [DataMember]
        public string BindPort { get; set; }
        
        public PivotHandler(string handlerName, string hostname, string bindPort)
        {
            HandlerName = handlerName;
            Hostname = hostname;
            BindPort = bindPort;
        }
    }
}