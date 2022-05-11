using TeamServer.Handlers;

namespace TeamServer.Interfaces;

public interface IHandlerService
{
    // create
    Task AddHandler(Handler handler);
    
    // read
    Handler GetHandler(string name);
    IEnumerable<Handler> GetHandlers();
    
    // update
    Task UpdateHandler(Handler handler);
    
    // delete
    Task DeleteHandler(Handler handler);
}