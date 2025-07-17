using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Infrastructure.Persistent;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests
{
    public class EndToEndUserFlowTests : TestBase
    {
        public EndToEndUserFlowTests(CustomWebApplicationFactory<Program> factory)
            : base(factory) { }

        [Fact]
        public async Task EndToEnd_UserFlow_WorksAsExpected()
        {
            // 1. Hello endpoint
            var helloResponse = await Client.GetAsync("/tests/hello");
            helloResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var helloContent = await helloResponse.Content.ReadAsStringAsync();
            var helloJson = JsonDocument.Parse(helloContent);
            helloJson
                .RootElement.GetProperty("value")
                .GetProperty("message")
                .GetString()
                .Should()
                .Be("Hello from FastEndpoints!");

            // 2. Get users without auth
            var usersResponse = await Client.GetAsync("/administration/users");
            usersResponse
                .StatusCode.Should()
                .BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);

            // 3. Register new user
            var unique = Guid.NewGuid().ToString("N").Substring(0, 8);
            var registerRequest = new
            {
                userName = $"e2euser_{unique}",
                email = $"e2e_{unique}@example.com",
                password = "E2eTest123!",
            };
            var registerResponse = await Client.PostAsJsonAsync("/auth/register", registerRequest);
            registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            using var registerDoc = JsonDocument.Parse(registerContent);
            registerDoc
                .RootElement.TryGetProperty("isSuccess", out var isSuccessProp)
                .Should()
                .BeTrue();
            isSuccessProp.GetBoolean().Should().BeTrue();
            registerDoc.RootElement.TryGetProperty("value", out var valueProp).Should().BeTrue();
            Guid.TryParse(valueProp.GetString(), out var guid).Should().BeTrue();
            guid.Should().NotBe(Guid.Empty);

            // 3.1. Назначить роль пользователю через репозиторий
            var scopeFactory = Factory.Services.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var user = await userRepository.FindByEmailAsync(registerRequest.email);
                user.Should().NotBeNull();
                var role = await roleRepository.FindByNameAsync("Admin");
                role.Should().NotBeNull();
                user!.AssignRole(role!);
                await db.SaveChangesAsync();
            }

            // 4. Login
            var loginRequest = new
            {
                email = registerRequest.email,
                password = registerRequest.password,
            };
            var loginResponse = await Client.PostAsJsonAsync("/auth/login", loginRequest);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            using var loginDoc = JsonDocument.Parse(loginContent);
            loginDoc
                .RootElement.TryGetProperty("isSuccess", out var loginIsSuccessProp)
                .Should()
                .BeTrue();
            loginIsSuccessProp.GetBoolean().Should().BeTrue();
            loginDoc.RootElement.TryGetProperty("value", out var loginValueProp).Should().BeTrue();
            loginValueProp.ValueKind.Should().Be(JsonValueKind.Object);
            loginValueProp.TryGetProperty("accessToken", out var accessTokenProp).Should().BeTrue();
            var accessToken = accessTokenProp.GetString();
            accessToken.Should().NotBeNullOrWhiteSpace();

            // 5. Get users with auth
            Client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var usersWithAuthResponse = await Client.GetAsync("/administration/users");
            usersWithAuthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var usersContent = await usersWithAuthResponse.Content.ReadAsStringAsync();
            using var usersDoc = JsonDocument.Parse(usersContent);
            usersDoc
                .RootElement.TryGetProperty("isSuccess", out var usersIsSuccessProp)
                .Should()
                .BeTrue();
            usersIsSuccessProp.GetBoolean().Should().BeTrue();
            usersDoc.RootElement.TryGetProperty("value", out var usersValueProp).Should().BeTrue();
            usersValueProp.ValueKind.Should().Be(JsonValueKind.Array);
        }
    }
}
