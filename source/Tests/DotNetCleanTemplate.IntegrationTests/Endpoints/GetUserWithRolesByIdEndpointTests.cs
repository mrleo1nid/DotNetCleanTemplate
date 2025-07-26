using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.IntegrationTests.Common;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace DotNetCleanTemplate.IntegrationTests.Endpoints
{
    public class GetUserWithRolesByIdEndpointTests : TestBase
    {
        public GetUserWithRolesByIdEndpointTests(CustomWebApplicationFactory<Program> factory)
            : base(factory) { }

        [Fact]
        public async Task GetUserWithRolesById_WithValidUserId_ReturnsUserWithRoles()
        {
            // Arrange
            var email = "admin@example.com";
            var password = "AdminPassword123!";
            var token = await AuthenticateAsync(email, password);
            Client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var usersResponse = await Client!.GetAsync("/administration/users");
            var usersResult = await usersResponse.Content.ReadFromJsonAsync<
                Result<List<UserWithRolesDto>>
            >();
            var firstUser = usersResult?.Value?.FirstOrDefault();

            firstUser.Should().NotBeNull();

            // Act
            var response = await Client!.GetAsync(
                $"/administration/users/{firstUser!.Id}/with-roles"
            );

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<Result<UserWithRolesDto>>();
            result.Should().NotBeNull();
            result!.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().Be(firstUser.Id);
            result.Value.UserName.Should().Be(firstUser.UserName);
            result.Value.Email.Should().Be(firstUser.Email);
            result.Value.Roles.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUserWithRolesById_WithInvalidUserId_ReturnsNotFound()
        {
            // Arrange
            var email = "admin@example.com";
            var password = "AdminPassword123!";
            var token = await AuthenticateAsync(email, password);
            Client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var invalidUserId = Guid.NewGuid();

            // Act
            var response = await Client!.GetAsync(
                $"/administration/users/{invalidUserId}/with-roles"
            );

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<Result<UserWithRolesDto>>();
            result.Should().NotBeNull();
            result!.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Code == ErrorCodes.UserNotFound);
        }

        [Fact]
        public async Task GetUserWithRolesById_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var response = await Client!.GetAsync($"/administration/users/{userId}/with-roles");

            // Assert
            response
                .StatusCode.Should()
                .BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        }
    }
}
