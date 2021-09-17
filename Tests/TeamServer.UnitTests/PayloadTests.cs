using System.Threading.Tasks;

using TeamServer.Interfaces;
using TeamServer.Models;

using Xunit;

namespace TeamServer.UnitTests
{
    public class PayloadTests
    {
        private readonly IHandlerService _handlers;
        private readonly IServerService _server;
        
        public PayloadTests(IHandlerService handlers, IServerService server)
        {
            handlers.LoadDefaultHandlers();
            
            _handlers = handlers;
            _server = server;
        }

        [Fact]
        public async Task GenerateExePayload()
        {
            var handler = _handlers.GetHandler("default-http");
            var profile = _server.GetC2Profile();
            
            var payload = new ExePayload(handler, profile);
            await payload.Generate();
            
            Assert.NotNull(payload.Bytes);
            Assert.True(payload.Bytes.Length > 0);
        }
        
        [Fact]
        public async Task GenerateDllPayload()
        {
            var handler = _handlers.GetHandler("default-http");
            var profile = _server.GetC2Profile();
            
            var payload = new DllPayload(handler, profile);
            await payload.Generate();
            
            Assert.NotNull(payload.Bytes);
            Assert.True(payload.Bytes.Length > 0);
        }
        
        [Fact]
        public async Task GeneratePowerShellPayload()
        {
            var handler = _handlers.GetHandler("default-http");
            var profile = _server.GetC2Profile();
            
            var payload = new PoshPayload(handler, profile);
            await payload.Generate();
            
            Assert.NotNull(payload.Bytes);
            Assert.True(payload.Bytes.Length > 0);
        }
        
        [Fact]
        public async Task GenerateRawPayload()
        {
            var handler = _handlers.GetHandler("default-http");
            var profile = _server.GetC2Profile();
            
            var payload = new RawPayload(handler, profile);
            await payload.Generate();
            
            Assert.NotNull(payload.Bytes);
            Assert.True(payload.Bytes.Length > 0);
        }
        
        [Fact]
        public async Task GenerateSvcPayload()
        {
            var handler = _handlers.GetHandler("default-http");
            var profile = _server.GetC2Profile();
            
            var payload = new ServicePayload(handler, profile);
            await payload.Generate();
            
            Assert.NotNull(payload.Bytes);
            Assert.True(payload.Bytes.Length > 0);
        }
    }
}