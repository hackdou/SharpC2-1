using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

using SharpC2.API.V1;
using SharpC2.API.V1.Requests;

using Xunit;

namespace SharpC2.API.IntegrationTests
{
    public class HostedFileTests : IntegrationTest
    {
        private const string TestFile = "dGhpcyBpcyBhIHRlc3QgZmlsZQ==";
        
        [Fact]
        public async Task AddFile()
        {
            const string handlerName = "test-http";
            
            await CreateHandler(handlerName);
            
            var request = new AddHostedFileRequest
            {
                Content = Convert.FromBase64String(TestFile),
                Filename = "test.txt"
            };

            var response = await Client.PostAsJsonAsync($"{Routes.V1.HostedFiles}/{handlerName}", request);
            var found = response.Headers.TryGetValues("Location", out var location);

            location = location?.ToArray();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(found);
            Assert.NotEmpty(location);
        }
    }
}