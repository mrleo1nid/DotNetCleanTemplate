using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.IntegrationTests.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DotNetCleanTemplate.IntegrationTests.Endpoints
{
    public class AssignRoleToUserEndpointTests : TestBase
    {
        public AssignRoleToUserEndpointTests(CustomWebApplicationFactory<Program> factory)
            : base(factory) { }

        [Fact]
        public async Task AssignRoleToUser_WithoutAuth_ReturnsUnauthorizedOrForbidden()
        {
            var dto = new AssignRoleToUserDto { UserId = Guid.NewGuid(), RoleId = Guid.NewGuid() };
            var response = await Client!.PostAsJsonAsync("/administration/assign-role", dto);
            response
                .StatusCode.Should()
                .BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AssignRoleToUser_WithAdminAuth_ReturnsSuccessOrFailure()
        {
            var email = "admin@example.com";
            var password = "AdminPassword123!";
            var token = await AuthenticateAsync(email, password);
            Client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Для теста нужны существующие userId и roleId. Здесь пример с несуществующими id.
            var dto = new AssignRoleToUserDto { UserId = Guid.NewGuid(), RoleId = Guid.NewGuid() };
            var response = await Client!.PostAsJsonAsync("/administration/assign-role", dto);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            doc.RootElement.TryGetProperty("isSuccess", out var _).Should().BeTrue();
            // isSuccess может быть true или false в зависимости от наличия user/role
        }
    }
}
