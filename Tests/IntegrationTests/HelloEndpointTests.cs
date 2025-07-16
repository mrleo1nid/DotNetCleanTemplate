using System.Net;
using System.Text.Json;
using DotNetCleanTemplate.Api;
using FluentAssertions;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class HelloEndpointTests : TestBase
    {
        public HelloEndpointTests(
            CustomWebApplicationFactory<Program> factory,
            ITestOutputHelper output
        )
            : base(factory, output) { }

        [Fact]
        public async Task HelloEndpoint_ReturnsHelloMessage()
        {
            // Act
            var response = await Client.GetAsync("/hello");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var message = json.RootElement.GetProperty("value").GetProperty("message").GetString();

            message.Should().Be("Hello from FastEndpoints!");
        }
    }
}
