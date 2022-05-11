using Client.Models;
using System.Reflection;
using System.Text;

using YamlDotNet.Serialization;

namespace Client.Services
{
    public class SharpC2Commands
    {
        private readonly List<DroneCommand> _commands = new();

        public SharpC2Commands()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(location);
            var path = Path.Combine(directory, "Commands");
            var files = Directory.GetFiles(path, "*.yaml");

            var serializer = new Deserializer();
            
            foreach (var file in files)
            {
                try
                {
                    var text = File.ReadAllText(file);
                    var commands = serializer.Deserialize<DroneCommand[]>(text);

                    if (commands is null || commands.Length == 0)
                        continue;

                    _commands.AddRange(commands);
                }
                catch
                {
                    // ignore
                }
            }
        }

        public string GetHelp()
        {
            var list = new ResultList<DroneCommand>();
            var commands = _commands.OrderBy(c => c.Alias);

            list.AddRange(commands);
            return list.ToString();
        }

        public string GetHelp(string alias)
        {
            var command = _commands.FirstOrDefault(c => c.Alias.Equals(alias, StringComparison.OrdinalIgnoreCase));
            if (command is null) return string.Empty;

            var sb = new StringBuilder();

            sb.AppendLine($"Alias: {command.Alias}");
            sb.AppendLine($"Descrtipion: {command.Description}");
            sb.AppendLine($"Usage: {command.Usage}");

            return sb.ToString().TrimEnd();
        }

        public DroneCommand GetCommand(string alias)
        {
            return _commands.FirstOrDefault(c => c.Alias.Equals(alias, StringComparison.OrdinalIgnoreCase));
        }
    }
}