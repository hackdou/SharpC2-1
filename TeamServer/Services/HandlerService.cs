using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.AspNetCore.SignalR;

using TeamServer.Handlers;
using TeamServer.Hubs;
using TeamServer.Interfaces;

namespace TeamServer.Services
{
    public class HandlerService : IHandlerService
    {
        private readonly List<Handler> _handlers = new();
        
        private readonly ITaskService _taskService;
        private readonly IServerService _serverService;
        private readonly IHubContext<MessageHub, IMessageHub> _hubContext;

        public HandlerService(ITaskService taskService, IServerService serverService, IHubContext<MessageHub, IMessageHub> hubContext)
        {
            _taskService = taskService;
            _serverService = serverService;
            _hubContext = hubContext;
        }

        private IEnumerable<Handler> LoadHandler(Assembly asm)
        {
            List<Handler> handlers = new();
            
            foreach (var type in asm.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(Handler))) continue;

                if (Activator.CreateInstance(type) is not Handler handler)
                    throw new Exception("Could not create instance of Handler.");

                RegisterHandler(handler);
                handlers.Add(handler);
            }

            return handlers;
        }

        public void AddHandler(Handler handler)
        {
            _handlers.Add(handler);
        }

        public Handler LoadHandler(byte[] bytes)
        {
            var asm = Assembly.Load(bytes);
            var handlers = LoadHandler(asm);

            return handlers.First();
        }

        private void RegisterHandler(Handler handler)
        {
            handler.Init(_taskService, _serverService, _hubContext);
            _handlers.Add(handler);
        }

        public IEnumerable<Handler> GetHandlers()
        {
            return _handlers;
        }

        public Handler GetHandler(string name)
        {
            return GetHandlers()
                .FirstOrDefault(h => h.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public bool RemoveHandler(string name)
        {
            var handler = GetHandler(name);
            handler?.Stop();

            return _handlers.Remove(handler);
        }
    }
}