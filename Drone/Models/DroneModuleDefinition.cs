using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Drone.Models
{
    [DataContract]
    public class DroneModuleDefinition
    {
        [DataMember (Name = "name")]
        public string Name { get; set; }
        
        [DataMember (Name = "commands")]
        public List<CommandDefinition> Commands { get; set; } = new();

        [DataContract]
        public class CommandDefinition
        {
            [DataMember (Name = "name")]
            public string Name { get; set; }
            
            [DataMember (Name = "description")]
            public string Description { get; set; }
            
            [DataMember (Name = "arguments")]
            public List<ArgumentDefinition> Arguments { get; set; } = new();

            [DataContract]
            public class ArgumentDefinition
            {
                [DataMember (Name = "label")]
                public string Label { get; set; }
                
                [DataMember (Name = "artefact")]
                public bool Artefact { get; set; }
                
                [DataMember (Name = "optional")]
                public bool Optional { get; set; }
            }
        }
    }
}