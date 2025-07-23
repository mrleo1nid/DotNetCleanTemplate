using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.IntegrationTests.Common;
using FluentAssertions;
using System.Net;
using System.Text.Json;

namespace DotNetCleanTemplate.IntegrationTests.Endpoints
{
    public class HelloEndpointTests : TestBase
    {
        public HelloEndpointTests(CustomWebApplicationFactory<Program> factory)
            : base(factory) { }

        [Fact]
        public async Task HelloEndpoint_ReturnsHelloMessage()
        {
            // Act
            var response = await Client.GetAsync("/tests/hello");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var message = json.RootElement.GetProperty("value").GetProperty("message").GetString();

            message.Should().Be("Hello from FastEndpoints!");
        }
    }
}
