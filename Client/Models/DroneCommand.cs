
using System.Text;

namespace Client.Models;

public class DroneCommand : Result
{
    public string Alias { get; set; } = "";
    public string Description { get; set; } = "";

    public string Usage
    {
        get
        {
            var sb = new StringBuilder();
            sb.Append($"{Alias} ");

            foreach (var argument in Arguments)
            {
                if (!argument.Visible)
                    continue;

                sb.Append(argument.Optional ? "[" : "<");
                sb.Append(argument.Name);
                sb.Append(argument.Optional ? "] " : "> ");
            }

            return sb.ToString().TrimEnd();
        }
    }

    public string Function { get; set; } = "";
    public CommandArgument[] Arguments { get; set; } = Array.Empty<CommandArgument>();

    public class CommandArgument
    {
        public string Name { get; set; } = "";
        public string DefaultValue { get; set; } = "";
        public bool Optional { get; set; } = true;
        public bool Artefact { get; set; } = false;
        public bool Embedded { get; set; } = false;
        public bool Visible { get; set; } = true;
    }

    protected internal override IList<ResultProperty> ResultProperties => new List<ResultProperty>
    {
        new ResultProperty { Name = "Alias", Value = Alias },
        new ResultProperty { Name = "Description", Value = Description }
    };
}