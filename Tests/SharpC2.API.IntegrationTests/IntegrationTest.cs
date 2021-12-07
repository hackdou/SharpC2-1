using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SharpC2.API.V1;
using SharpC2.API.V1.Requests;

using TeamServer;
using TeamServer.Interfaces;
using TeamServer.Services;

namespace SharpC2.API.IntegrationTests
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

            var serverService = factory.Services.GetRequiredService<SharpC2Service>();
            serverService.LoadDefaultModules();

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
        
        protected async Task CreateHandler(string name)
        {
            var request = new CreateHandlerRequest
            {
                HandlerName = name,
                Type = CreateHandlerRequest.HandlerType.HTTP
            };

            await Client.PostAsJsonAsync(Routes.V1.Handlers, request);
        }
    }    
}