using System.Threading.Tasks;

using TeamServer.Handlers;
using TeamServer.Models;

namespace TeamServer.Modules
{
    public class PivotModule : Module
    {
        public override string Name => "Pivot";

        public override Task Execute(DroneMetadata metadata, DroneTaskUpdate update)
        {
            var pivot = update.Result.Deserialize<PivotHandler>();
            if (pivot is null) return Task.CompletedTask;

            var handler = new TcpHandler(pivot.HandlerName);
            handler.SetParameter("BindPort", pivot.BindPort);
            handler.SetParameter("LocalhostOnly", "false");
            handler.SetParameter("ConnectAddress", pivot.Hostname);
            
            Server.AddHandler(handler);
            
            return Task.CompletedTask;
        }
    }
}