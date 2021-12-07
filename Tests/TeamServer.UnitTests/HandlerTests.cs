using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using TeamServer.Handlers;
using TeamServer.Services;

using Xunit;

namespace TeamServer.UnitTests
{
    public class HandlerTests
    {
        private readonly SharpC2Service _server;

        public HandlerTests(SharpC2Service server)
        {
            _server = server;
        }

        [Fact]
        public void CreateHandler()
        {
            const string handlerName = "test";
            
            var httpHandler = new HttpHandler(handlerName);
            _server.AddHandler(httpHandler);

            var handler = _server.GetHandler(handlerName);
            
            Assert.Equal(httpHandler, handler);
        }

        [Fact]
        public void RemoveHandler()
        {
            const string handlerName = "test";
            
            var httpHandler = new HttpHandler(handlerName);
            _server.AddHandler(httpHandler);
            _server.RemoveHandler(httpHandler);

            var handler = _server.GetHandler(handlerName);
            
            Assert.Null(handler);
        }

        [Fact]
        public async Task StartHttpHandler()
        {
            const string handlerName = "test";
            
            var httpHandler = new HttpHandler(handlerName);
            httpHandler.Init(_server);
            httpHandler.Start();

            using var client = new HttpClient { BaseAddress = new Uri("http://localhost") };
            var response = await client.GetAsync("/");
            
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}