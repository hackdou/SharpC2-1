using System.Collections.Generic;

namespace Drone.Modules
{
    public abstract class DroneModule
    {
        public abstract string Name { get; }
        
        public List<Command> Commands { get; } = new();

        protected Drone Drone;
        protected DroneConfig Config;

        public void Init(Drone drone, DroneConfig config)
        {
            Drone = drone;
            Config = config;
        }

        public abstract void AddCommands();

        public class Command
        {
            public string Name { get; }
            public string Description { get; }
            public List<Argument> Arguments { get; } = new();
            public Drone.Callback Callback { get; }

            public Command(string name, string description, Drone.Callback callback)
            {
                Name = name;
                Description = description;
                Callback = callback;
            }

            public class Argument
            {
                public string Label { get; }
                public bool Artefact { get; }
                public bool Optional { get; }

                public Argument(string label, bool optional = true, bool artefact = false)
                {
                    Label = label;
                    Artefact = artefact;
                    Optional = optional;
                }
            }
        }
    }
}