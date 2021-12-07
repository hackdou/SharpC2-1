using System.Net.Http.Json;
using System.Threading.Tasks;

using SharpC2.API.V1;
using SharpC2.API.V1.Responses;

using Xunit;

namespace SharpC2.API.IntegrationTests
{
    public class PayloadTests : IntegrationTest
    {
        [Fact]
        public async Task GeneratePayload()
        {
            const string handlerName = "test-http";
            
            await CreateHandler(handlerName);
            
            var payloadResponse = await Client.GetFromJsonAsync<PayloadResponse>($"{Routes.V1.Payloads}/{handlerName}/exe");
            
            Assert.NotNull(payloadResponse);
            Assert.True(payloadResponse.Bytes?.Length > 0);
        }
    }
}