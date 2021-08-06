using System.Text;

namespace SharpC2.Models
{
    public class DroneModule
    {
        public string Name { get; set; }
        public Command[] Commands { get; set; }

        public class Command
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public Argument[] Arguments { get; set; }

            public string Usage
            {
                get
                {
                    var sb = new StringBuilder($"{Name} ");
                    
                    foreach (var argument in Arguments)
                    {
                        sb.Append(argument.Optional ? "<" : "[");
                        sb.Append(argument.Label);
                        sb.Append(argument.Optional ? ">" : "]");
                        sb.Append(' ');
                    }

                    return sb.ToString().TrimEnd();
                }
            }

            public class Argument
            {
                public string Label { get; set; }
                public bool Artefact { get; set; }
                public bool Optional { get; set; }
            }
        }
    }
}