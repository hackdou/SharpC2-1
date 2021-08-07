using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using TeamServer.Handlers;
using TeamServer.Interfaces;

namespace TeamServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // args => <password> [handler.dll]
            if (args.Length < 1)
                throw new ArgumentException("Usage: TeamServer.exe <password>");

            var host = CreateHostBuilder(args).Build();

            // Set the server password for user auth
            var userService = host.Services.GetRequiredService<IUserService>();
            userService.SetPassword(args[0]);

            // Always load the default handler
            var handlerService = host.Services.GetRequiredService<IHandlerService>();
            handlerService.LoadDefaultHandlers();

            // If path to handler was included, load it
            if (args.Length > 1)
            {
                var bytes = await File.ReadAllBytesAsync(args[1]);
                handlerService.LoadHandler(bytes);
            }

            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                    { webBuilder.UseStartup<Startup>();});
    }
}