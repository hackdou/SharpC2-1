using System;
using System.Linq;
using System.Net.Http;
using System.Text;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using TeamServer;
using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Services;

namespace API.IntegrationTests
{
    public class IntegrationTest
    {
        protected readonly HttpClient Client;

        protected IntegrationTest()
        {
            var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes("rasta:Passw0rd!"));

            var factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(ConfigureServices);
                });

            var handlerService = factory.Services.GetRequiredService<IHandlerService>();
            handlerService.LoadDefaultHandlers();

            var serverService = factory.Services.GetRequiredService<IServerService>();
            serverService.SetC2Profile(new C2Profile());

            Client = factory.CreateClient();
            Client.DefaultRequestHeaders.Add("Authorization", $"Basic {basic}");
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.RemoveAll(typeof(IUserService));

            var userService = new UserService();
            userService.SetPassword("Passw0rd!");

            services.AddSingleton<IUserService>(userService);
        }
    }    
}