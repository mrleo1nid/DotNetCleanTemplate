using System.Net;
using System.Text.Json;
using DotNetCleanTemplate.Api;
using FluentAssertions;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class ErrorHandlingMiddlewareTests : TestBase
    {
        public ErrorHandlingMiddlewareTests(CustomWebApplicationFactory<Program> factory)
            : base(factory) { }

        [Fact]
        public async Task ThrowErrorEndpoint_ReturnsInternalServerErrorWithErrorMessage()
        {
            var response = await Client!.GetAsync("/throw-error");
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            doc.RootElement.TryGetProperty("error", out var errorProp).Should().BeTrue();
            errorProp.GetString().Should().Be("An unexpected error occurred.");
            doc.RootElement.TryGetProperty("details", out var detailsProp).Should().BeTrue();
            detailsProp.GetString().Should().Contain("Test exception for middleware");
        }
    }
}
