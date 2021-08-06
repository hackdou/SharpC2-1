using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using TeamServer.Models;

namespace TeamServer.Handlers
{
    public class DefaultHttpHandler : Handler
    {
        public override string Name { get; } = "default-http";

        public override List<HandlerParameter> Parameters { get; } = new()
        {
            new HandlerParameter("BindPort", "80", false),
            new HandlerParameter("ConnectAddress", "localhost", false),
            new HandlerParameter("ConnectPort", "80", false)
        };

        public override Task Start()
        {
            // this throws if the handler doesn't have the required parameters set
            base.Start();

            TokenSource = new CancellationTokenSource();

            var host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls($"http://0.0.0.0:{GetParameter("BindPort")}");
                    webBuilder.Configure(ConfigureApp);
                    webBuilder.ConfigureServices(ConfigureServices);
                    webBuilder.ConfigureKestrel(ConfigureKestrel);
                })
                .Build();

            return host.RunAsync(TokenSource.Token);
        }

        private static void ConfigureKestrel(KestrelServerOptions k)
        {
            k.AddServerHeader = false;
        }

        private static void ConfigureApp(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(e =>
            {
                e.MapControllerRoute("/", "/", new
                {
                    controller = "DefaultHttpHandler",
                    action = "RouteDrone"
                });
            });
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton(MessageHub);
            services.AddSingleton(TaskService);
        }

        public override void Stop()
        {
            TokenSource.Cancel();
            TokenSource.Dispose();
        }
    }
}