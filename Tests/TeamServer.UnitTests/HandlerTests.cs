using System.Linq;

using TeamServer.Interfaces;

using Xunit;

namespace TeamServer.UnitTests
{
    public class HandlerTests
    {
        private readonly IHandlerService _handlers;

        public HandlerTests(IHandlerService handlers)
        {
            handlers.LoadDefaultHandlers();
            _handlers = handlers;
        }

        [Fact]
        public void GetHandlers()
        {
            var handlers = _handlers.GetHandlers();
            
            Assert.True(handlers.Any());
        }

        [Fact]
        public void GetHandler()
        {
            const string handlerName = "default-http";

            var handler = _handlers.GetHandler(handlerName);
            
            Assert.NotNull(handler);
            Assert.Equal(handlerName, handler.Name);
        }
    }
}