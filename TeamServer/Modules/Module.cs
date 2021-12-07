using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;

using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer.Modules
{
    public abstract class Module
    {
        public abstract string Name { get; }

        protected SharpC2Service Server;
        protected IHubContext<MessageHub, IMessageHub> MessageHub;

        public void Init(SharpC2Service server, IHubContext<MessageHub, IMessageHub> hub)
        {
            Server = server;
            MessageHub = hub;
        }
        
        public abstract Task Execute(DroneMetadata metadata, DroneTaskUpdate update);
    }
}