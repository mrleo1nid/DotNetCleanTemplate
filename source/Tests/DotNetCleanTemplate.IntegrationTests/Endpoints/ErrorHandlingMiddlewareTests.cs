using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.IntegrationTests.Common;
using FluentAssertions;
using System.Net;
using System.Text.Json;

namespace DotNetCleanTemplate.IntegrationTests.Endpoints
{
    public class ErrorHandlingMiddlewareTests : TestBase
    {
        public ErrorHandlingMiddlewareTests(CustomWebApplicationFactory<Program> factory)
            : base(factory) { }

        [Fact]
        public async Task ThrowErrorEndpoint_ReturnsInternalServerErrorWithErrorMessage()
        {
            var response = await Client!.GetAsync("/tests/throw-error");
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            doc.RootElement.TryGetProperty("error", out var errorProp).Should().BeTrue();
            errorProp.GetString().Should().Be("An unexpected error occurred.");
            doc.RootElement.TryGetProperty("details", out var detailsProp).Should().BeTrue();
            detailsProp.GetString().Should().Contain("Test exception for middleware");
        }

        [Fact]
        public async Task ThrowErrorEndpoint_AlwaysThrowsException()
        {
            // Проверяем, что любой вызов приводит к ошибке
            var response = await Client!.GetAsync("/tests/throw-error");
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("An unexpected error occurred.");
        }
    }
}
