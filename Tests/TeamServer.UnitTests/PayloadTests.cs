using System.Threading.Tasks;

using TeamServer.Handlers;
using TeamServer.Services;

using Xunit;

namespace TeamServer.UnitTests
{
    public class PayloadTests
    {
        private readonly SharpC2Service _server;

        public PayloadTests(SharpC2Service server)
        {
            _server = server;
        }

        [Theory]
        [InlineData(SharpC2Service.PayloadFormat.Exe)]
        [InlineData(SharpC2Service.PayloadFormat.Dll)]
        [InlineData(SharpC2Service.PayloadFormat.Raw)]
        [InlineData(SharpC2Service.PayloadFormat.Svc)]
        [InlineData(SharpC2Service.PayloadFormat.PowerShell)]
        public async Task GeneratePayloadsHttpHandler(SharpC2Service.PayloadFormat format)
        {
            var handler = new HttpHandler("http");
            _server.AddHandler(handler);

            var payload = await _server.GeneratePayload(
                format,
                handler);
            
            Assert.NotNull(payload.Bytes);
            Assert.True(payload.Bytes.Length > 0);
        }
        
        [Theory]
        [InlineData(SharpC2Service.PayloadFormat.Exe)]
        [InlineData(SharpC2Service.PayloadFormat.Dll)]
        [InlineData(SharpC2Service.PayloadFormat.Raw)]
        [InlineData(SharpC2Service.PayloadFormat.Svc)]
        [InlineData(SharpC2Service.PayloadFormat.PowerShell)]
        public async Task GeneratePayloadsSmbHandler(SharpC2Service.PayloadFormat payloadFormat)
        {
            var handler = new SmbHandler("smb");
            _server.AddHandler(handler);

            var payload = await _server.GeneratePayload(
                payloadFormat,
                handler);
            
            Assert.NotNull(payload.Bytes);
            Assert.True(payload.Bytes.Length > 0);
        }
        
        [Theory]
        [InlineData(SharpC2Service.PayloadFormat.Exe)]
        [InlineData(SharpC2Service.PayloadFormat.Dll)]
        [InlineData(SharpC2Service.PayloadFormat.Raw)]
        [InlineData(SharpC2Service.PayloadFormat.Svc)]
        [InlineData(SharpC2Service.PayloadFormat.PowerShell)]
        public async Task GeneratePayloadsTcpHandler(SharpC2Service.PayloadFormat payloadFormat)
        {
            var handler = new TcpHandler("tcp");
            _server.AddHandler(handler);

            var payload = await _server.GeneratePayload(
                payloadFormat,
                handler);
            
            Assert.NotNull(payload.Bytes);
            Assert.True(payload.Bytes.Length > 0);
        }
    }
}