using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.IntegrationTests.Common;
using FluentAssertions;
using System.Net;
using System.Text.Json;

namespace DotNetCleanTemplate.IntegrationTests.Endpoints
{
    public class GetAllRolesEndpointTests : TestBase
    {
        public GetAllRolesEndpointTests(CustomWebApplicationFactory<Program> factory)
            : base(factory) { }

        [Fact]
        public async Task GetAllRoles_WithoutAuth_ReturnsUnauthorizedOrForbidden()
        {
            var response = await Client!.GetAsync("/administration/roles");
            response
                .StatusCode.Should()
                .BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetAllRoles_WithAdminAuth_ReturnsList()
        {
            var email = "admin@example.com";
            var password = "AdminPassword123!";
            var token = await AuthenticateAsync(email, password);
            Client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await Client!.GetAsync("/administration/roles");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            doc.RootElement.TryGetProperty("isSuccess", out var isSuccessProp).Should().BeTrue();
            isSuccessProp.GetBoolean().Should().BeTrue();
            doc.RootElement.TryGetProperty("value", out var valueProp).Should().BeTrue();
            valueProp.ValueKind.Should().Be(JsonValueKind.Array);
        }
    }
}
