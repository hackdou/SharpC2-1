using System.Net.Http.Json;
using System.Threading.Tasks;

using SharpC2.API.V1;
using SharpC2.API.V1.Requests;
using SharpC2.API.V1.Responses;

using Xunit;

namespace SharpC2.API.IntegrationTests
{
    public class HandlerTests : IntegrationTest
    {
        [Fact]
        public async Task CreateHttpHandler()
        {
            var request = new CreateHandlerRequest
            {
                HandlerName = "http-test",
                Type = CreateHandlerRequest.HandlerType.HTTP
            };

            var response = await Client.PostAsJsonAsync(Routes.V1.Handlers, request);
            var handler = await response.Content.ReadFromJsonAsync<HandlerResponse>();

            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(handler);
            Assert.Equal(request.HandlerName, handler.Name);
        }
    }
}