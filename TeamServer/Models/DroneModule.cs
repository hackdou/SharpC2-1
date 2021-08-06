using System;
using System.Collections.Generic;

namespace TeamServer.Models
{
    [Serializable]
    public class DroneModule
    {
        public string Name { get; set; }
        public IEnumerable<Command> Commands { get; set; }

        [Serializable]
        public class Command
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public IEnumerable<Argument> Arguments { get; set; }
            
            [Serializable]
            public class Argument
            {
                public string Label { get; set; }
                public bool Artefact { get; set; }
                public bool Optional { get; set; }
            }
        }
    }
}