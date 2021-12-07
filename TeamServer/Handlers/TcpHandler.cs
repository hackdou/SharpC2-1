using System.Collections.Generic;

using TeamServer.Models;

namespace TeamServer.Handlers
{
    public class TcpHandler : Handler
    {
        public override string Name { get; }

        public TcpHandler(string handlerName)
        {
            Name = handlerName;
        }

        public override List<HandlerParameter> Parameters { get; } = new()
        {
            new HandlerParameter("BindPort", "4444", false),
            new HandlerParameter("LocalhostOnly", "false", false),
            new HandlerParameter("ConnectAddress", "", true)
        };
        
        public override void Stop()
        {
            // does nothing
        }
    }
}