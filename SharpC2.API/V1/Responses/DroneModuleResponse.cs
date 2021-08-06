using System.Collections.Generic;

namespace SharpC2.API.V1.Responses
{
    public class DroneModuleResponse
    {
        public string Name { get; set; }
        public IEnumerable<CommandResponse> Commands { get; set; }

        public class CommandResponse
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public IEnumerable<ArgumentResponse> Arguments { get; set; }
            
            public class ArgumentResponse
            {
                public string Label { get; set; }
                public bool Artefact { get; set; }
                public bool Optional { get; set; }
            }
        }
    }
}