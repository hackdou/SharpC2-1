using System.Collections.Generic;

using TeamServer.Handlers;

namespace TeamServer.Interfaces
{
    public interface IHandlerService
    {
        void AddHandler(Handler handler);
        IEnumerable<Handler> LoadHandlers(byte[] bytes);
        IEnumerable<Handler> GetHandlers();
        Handler GetHandler(string name);
        void RemoveHandler(Handler handler);
    }
}