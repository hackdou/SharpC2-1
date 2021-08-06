using System.Collections.Generic;

using TeamServer.Handlers;

namespace TeamServer.Interfaces
{
    public interface IHandlerService
    {
        Handler LoadHandler(byte[] bytes);
        void LoadHandler(Handler handler);
        IEnumerable<Handler> GetHandlers();
        Handler GetHandler(string name);
        bool RemoveHandler(string name);
    }
}