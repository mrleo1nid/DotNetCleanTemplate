using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class RefreshTokenEndpointTests : TestBase
    {
        public RefreshTokenEndpointTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task RefreshToken_WithValidRefreshToken_ReturnsNewTokens()
        {
            // Сначала логинимся, чтобы получить refreshToken
            var loginRequest = new
            {
                email = "testuser@example.com",
                password = "TestPassword123!",
            };
            var loginResponse = await Client!.PostAsJsonAsync("/auth/login", loginRequest);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            using var loginDoc = JsonDocument.Parse(loginContent);
            loginDoc
                .RootElement.TryGetProperty("isSuccess", out var isSuccessProp)
                .Should()
                .BeTrue();
            if (!isSuccessProp.GetBoolean())
            {
                var error = loginDoc.RootElement.TryGetProperty("errors", out var errorsProp)
                    ? errorsProp.ToString()
                    : loginContent;
                throw new Xunit.Sdk.XunitException($"Login failed. Response: {loginContent}");
            }
            loginDoc.RootElement.TryGetProperty("value", out var loginValueProp).Should().BeTrue();
            if (loginValueProp.ValueKind != JsonValueKind.Object)
            {
                throw new Xunit.Sdk.XunitException(
                    $"Login value is not an object. Response: {loginContent}"
                );
            }
            var refreshToken = loginValueProp.TryGetProperty(
                "refreshToken",
                out var refreshTokenProp
            )
                ? refreshTokenProp.GetString()
                : null;
            refreshToken.Should().NotBeNullOrWhiteSpace();

            // Теперь делаем запрос на refresh
            var refreshRequest = new { refreshToken };
            var response = await Client.PostAsJsonAsync("/auth/refresh", refreshRequest);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            doc.RootElement.TryGetProperty("isSuccess", out var isSuccessProp2).Should().BeTrue();
            isSuccessProp2.GetBoolean().Should().BeTrue();
            doc.RootElement.TryGetProperty("value", out var valueProp).Should().BeTrue();
            valueProp.TryGetProperty("accessToken", out var accessTokenProp).Should().BeTrue();
            accessTokenProp.GetString().Should().NotBeNullOrWhiteSpace();
            valueProp.TryGetProperty("refreshToken", out var refreshTokenProp2).Should().BeTrue();
            refreshTokenProp2.GetString().Should().NotBeNullOrWhiteSpace();
            valueProp.TryGetProperty("expires", out var expiresProp).Should().BeTrue();
            expiresProp.ValueKind.Should().Be(JsonValueKind.String);
        }
    }
}
