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
        private readonly IHubContext<MessageHub, IMessageHub> _hubContext;

        public HandlerService(ITaskService taskService, IHubContext<MessageHub, IMessageHub> hubContext)
        {
            _taskService = taskService;
            _hubContext = hubContext;
        }

        public Handler LoadHandler(byte[] bytes)
        {
            var asm = Assembly.Load(bytes);

            foreach (var type in asm.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(Handler))) continue;

                if (Activator.CreateInstance(type) is not Handler handler)
                    throw new Exception("Could not create instance of Handler.");

                LoadHandler(handler);
                return handler;
            }

            return null;
        }

        public void LoadHandler(Handler handler)
        {
            handler.Init(_taskService, _hubContext);
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