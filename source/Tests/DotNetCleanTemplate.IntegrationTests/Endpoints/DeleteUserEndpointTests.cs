using System.Net;
using System.Net.Http.Json;
using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.IntegrationTests.Common;
using DotNetCleanTemplate.Shared.Common;
using MediatR;
using Xunit;

namespace DotNetCleanTemplate.IntegrationTests.Endpoints
{
    public class DeleteUserEndpointTests : TestBase
    {
        private const string AdminPassword = "AdminPassword123!";
        private const string AdminEmail = "admin@example.com";
        private const string BearerScheme = "Bearer";

        public DeleteUserEndpointTests(CustomWebApplicationFactory<Program> factory)
            : base(factory) { }

        [Fact]
        public async Task DeleteUser_WhenNotAuthenticated_ShouldReturnUnauthorized()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var response = await Client!.DeleteAsync($"/administration/users/{userId}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_WhenUserDoesNotExist_ShouldReturnError()
        {
            // Arrange
            var email = AdminEmail;
            var password = AdminPassword;
            var token = await AuthenticateAsync(email, password);
            Client!.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(BearerScheme, token);

            var nonExistentUserId = Guid.NewGuid();

            // Act
            var response = await Client.DeleteAsync($"/administration/users/{nonExistentUserId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<Result<Unit>>();
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.Errors);
        }
    }
}
