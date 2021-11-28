using System.Collections.Generic;

using TeamServer.Models;

namespace TeamServer.Handlers
{
    public class DefaultTcpHandler : Handler
    {
        public override string Name { get; } = "default-tcp";

        public override List<HandlerParameter> Parameters => new()
        {
            new HandlerParameter("BindPort", "4444", false),
            new HandlerParameter("LocalhostOnly", "false", false)
        };
        
        public override void Stop()
        {
            // does nothing
        }
    }
}