using Microsoft.AspNetCore.Server.Kestrel.Core;

using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer.Handlers;

public class HttpHandler : Handler
{
    private CancellationTokenSource _tokenSource;

    public int BindPort { get; set; }
    public string ConnectAddress { get; set; }
    public int ConnectPort { get; set; }
    
    public override HandlerType Type
        => HandlerType.Http;

    public HttpHandler(string name, int bindPort, string connectAddress, int connectPort, C2Profile profile)
        : base(name, profile)
    {
        BindPort = bindPort;
        ConnectAddress = connectAddress;
        ConnectPort = connectPort;
    }

    public override bool Running
    {
        get
        {
            if (_tokenSource is null) return false;
            return !_tokenSource.IsCancellationRequested;
        }
    }

    public override Task Start()
    {
        _tokenSource = new CancellationTokenSource();

        var host = new HostBuilder()
            .ConfigureWebHostDefaults(ConfigureWebHost)
            .Build();

        host.RunAsync(_tokenSource.Token);
        return Task.CompletedTask;
    }

    private void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls($"http://0.0.0.0:{BindPort}");
        builder.UseSetting("name", Name);
        builder.Configure(ConfigureApp);
        builder.ConfigureServices(ConfigureServices);
        builder.ConfigureKestrel(ConfigureKestrel);
    }

    private void ConfigureApp(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(ConfigureEndpoints);
    }

    private void ConfigureEndpoints(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapControllerRoute(Profile.Http.Endpoint, Profile.Http.Endpoint, new
        {
            controller = "HttpHandler", action = "RouteDrone"
        });
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<ICryptoService, CryptoService>();
        services.AddTransient<IDroneService, DroneService>();
        services.AddTransient<ITaskService, TaskService>();
        services.AddSingleton(Database);
        services.AddSingleton(Hub);
        services.AddControllers();
        services.AddAutoMapper(typeof(Program));
    }

    private static void ConfigureKestrel(KestrelServerOptions kestrel)
    {
        kestrel.AddServerHeader = false;
    }

    public override void Stop()
    {
        // if null or already cancelled
        if (_tokenSource is null || _tokenSource.IsCancellationRequested)
            return;

        _tokenSource.Cancel();
        _tokenSource.Dispose();
    }
}