using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Infrastructure.Persistent;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

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
            loginValueProp
                .TryGetProperty("refreshToken", out var refreshTokenProp)
                .Should()
                .BeTrue();
            var refreshToken = refreshTokenProp.GetString();
            refreshToken.Should().NotBeNullOrWhiteSpace();

            // 4.1. Проверить accessToken с защищённым эндпойнтом
            Client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var usersWithLoginTokenResponse = await Client.GetAsync("/administration/users");
            usersWithLoginTokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var usersWithLoginTokenContent =
                await usersWithLoginTokenResponse.Content.ReadAsStringAsync();
            using var usersWithLoginTokenDoc = JsonDocument.Parse(usersWithLoginTokenContent);
            usersWithLoginTokenDoc
                .RootElement.TryGetProperty("isSuccess", out var usersWithLoginTokenIsSuccessProp)
                .Should()
                .BeTrue();
            usersWithLoginTokenIsSuccessProp.GetBoolean().Should().BeTrue();
            usersWithLoginTokenDoc
                .RootElement.TryGetProperty("value", out var usersWithLoginTokenValueProp)
                .Should()
                .BeTrue();
            usersWithLoginTokenValueProp.ValueKind.Should().Be(JsonValueKind.Array);

            // 4.2. Получить новый accessToken по refreshToken
            var refreshRequest = new { refreshToken };
            var refreshResponse = await Client.PostAsJsonAsync("/auth/refresh", refreshRequest);
            refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var refreshContent = await refreshResponse.Content.ReadAsStringAsync();
            using var refreshDoc = JsonDocument.Parse(refreshContent);
            refreshDoc
                .RootElement.TryGetProperty("isSuccess", out var refreshIsSuccessProp)
                .Should()
                .BeTrue();
            refreshIsSuccessProp.GetBoolean().Should().BeTrue();
            refreshDoc
                .RootElement.TryGetProperty("value", out var refreshValueProp)
                .Should()
                .BeTrue();
            refreshValueProp
                .TryGetProperty("accessToken", out var newAccessTokenProp)
                .Should()
                .BeTrue();
            var newAccessToken = newAccessTokenProp.GetString();
            newAccessToken.Should().NotBeNullOrWhiteSpace();
            refreshValueProp
                .TryGetProperty("refreshToken", out var newRefreshTokenProp)
                .Should()
                .BeTrue();
            var newRefreshToken = newRefreshTokenProp.GetString();
            newRefreshToken.Should().NotBeNullOrWhiteSpace();
            refreshValueProp.TryGetProperty("expires", out var expiresProp).Should().BeTrue();
            expiresProp.ValueKind.Should().Be(JsonValueKind.String);

            // 5. Get users with auth (используем новый accessToken)
            Client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newAccessToken);
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
