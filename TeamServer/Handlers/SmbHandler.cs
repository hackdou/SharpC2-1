using System.Collections.Generic;

using TeamServer.Models;

namespace TeamServer.Handlers
{
    public class SmbHandler : Handler
    {
        public override string Name { get; }

        public SmbHandler(string handlerName)
        {
            Name = handlerName;
        }

        public override List<HandlerParameter> Parameters { get; } = new()
        {
            new HandlerParameter("PipeName", "SharpPipe", false)
        };

        public override void Stop()
        {
            // does nothing
        }
    }
}