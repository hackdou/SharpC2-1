using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;

using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Models;

namespace TeamServer.Modules
{
    public abstract class Module
    {
        public abstract string Name { get; }
        public abstract string Description { get; }

        protected IDroneService Drones;
        protected IHubContext<MessageHub, IMessageHub> Hub;

        public void Init(IDroneService drones, IHubContext<MessageHub, IMessageHub> hub)
        {
            Drones = drones;
            Hub = hub;
        }
        
        public abstract Task Execute(DroneMetadata metadata, DroneTaskUpdate update);
    }
}