using Microsoft.AspNetCore.SignalR;

using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer.Handlers;

public abstract class Handler
{
    public string Name { get; set; }
    public C2Profile Profile { get; set; }
    
    protected IDatabaseService Database { get; private set; }
    protected IHubContext<HubService, IHubService> Hub { get; private set; }

    protected Handler(string name, C2Profile profile)
    {
        Name = name;
        Profile = profile;
    }

    public void Init(IDatabaseService db, IHubContext<HubService, IHubService> hub)
    {
        Database = db;
        Hub = hub;
    }

    public abstract HandlerType Type { get; }
    public abstract bool Running { get; }

    public abstract Task Start();
    public abstract void Stop();
    
    public enum HandlerType : int
    {
        Http = 0,
        Dns = 1,
        Tcp = 2,
        Smb = 3,
        External = 4
    }
}