using AutoMapper;

using Microsoft.AspNetCore.SignalR;

using TeamServer.Handlers;
using TeamServer.Interfaces;
using TeamServer.Storage;

namespace TeamServer.Services;

public class HandlerService : IHandlerService
{
    private readonly IMapper _mapper;
    private readonly IDatabaseService _db;
    private readonly IProfileService _profiles;
    private IHubContext<HubService, IHubService> _hub;

    private readonly List<Handler> _handlers = new();

    public HandlerService(IMapper mapper, IDatabaseService db, IHubContext<HubService, IHubService> hub,
        IProfileService profiles)
    {
        _mapper = mapper;
        _db = db;
        _hub = hub;
        _profiles = profiles;

        // load handlers from db
        LoadHandlersFromDb().GetAwaiter().GetResult();
    }

    private async Task LoadHandlersFromDb()
    {
        var conn = _db.GetAsyncConnection();
        var http = await conn.Table<HttpHandlerDao>().ToArrayAsync();
        
        foreach (var dao in http)
        {
            var profile = await _profiles.GetProfile(dao.Profile);
            
            if (profile is null)
                continue;
            
            var handler = new HttpHandler(dao.Name, dao.BindPort, dao.ConnectAddress, dao.ConnectPort, profile);
            handler.Init(_db, _hub);
            
            _handlers.Add(handler);
        }
    }

    public async Task AddHandler(Handler handler)
    {
        _handlers.Add(handler);

        var conn = _db.GetAsyncConnection();
        
        switch (handler.Type)
        {
            case Handler.HandlerType.Http:
                var httpDao = _mapper.Map<HttpHandler, HttpHandlerDao>(handler as HttpHandler);
                await conn.InsertAsync(httpDao);
                break;
            
            case Handler.HandlerType.Dns:
                break;
            
            case Handler.HandlerType.Tcp:
                break;
            
            case Handler.HandlerType.Smb:
                break;
        }
    }

    public Handler GetHandler(string name)
    {
        return _handlers.FirstOrDefault(h => h.Name.Equals(name));
    }

    public IEnumerable<Handler> GetHandlers()
    {
        return _handlers;
    }

    public async Task UpdateHandler(Handler handler)
    {
        var conn = _db.GetAsyncConnection();
        
        switch (handler.Type)
        {
            case Handler.HandlerType.Http:
                var httpDao = _mapper.Map<HttpHandler, HttpHandlerDao>(handler as HttpHandler);
                await conn.UpdateAsync(httpDao);
                break;
            
            case Handler.HandlerType.Dns:
                break;
            
            case Handler.HandlerType.Tcp:
                break;
            
            case Handler.HandlerType.Smb:
                break;
        }
    }

    public async Task DeleteHandler(Handler handler)
    {
        _handlers.Remove(handler);
        
        var conn = _db.GetAsyncConnection();
        
        switch (handler.Type)
        {
            case Handler.HandlerType.Http:
                var httpDao = _mapper.Map<HttpHandler, HttpHandlerDao>(handler as HttpHandler);
                await conn.DeleteAsync(httpDao);
                break;
            
            case Handler.HandlerType.Dns:
                break;
            
            case Handler.HandlerType.Tcp:
                break;
            
            case Handler.HandlerType.Smb:
                break;
        }
    }
}