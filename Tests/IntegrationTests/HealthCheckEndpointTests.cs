using System.Net;
using System.Text.Json;
using DotNetCleanTemplate.Api;
using FluentAssertions;

namespace IntegrationTests
{
    public class HealthCheckEndpointTests : TestBase
    {
        public HealthCheckEndpointTests(CustomWebApplicationFactory<Program> factory)
            : base(factory) { }

        [Fact]
        public async Task HealthCheck_ReturnsHealthyStatus()
        {
            // Act
            var response = await Client.GetAsync("/health");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var status = json.RootElement.GetProperty("status").GetInt32();
            var dbStatus = json.RootElement.GetProperty("databaseStatus").GetInt32();
            var cacheStatus = json.RootElement.GetProperty("cacheStatus").GetInt32();
            var serverTime = json.RootElement.GetProperty("serverTime").GetDateTime();

            status.Should().Be(0); // Healthy
            dbStatus.Should().Be(0); // Healthy
            cacheStatus.Should().Be(0); // Healthy
            serverTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        }
    }
}
