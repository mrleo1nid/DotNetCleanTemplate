using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.IntegrationTests.Common;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DotNetCleanTemplate.IntegrationTests.Endpoints
{
    public class LoginEndpointTests : TestBase
    {
        public LoginEndpointTests(CustomWebApplicationFactory<Program> factory)
            : base(factory) { }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsTokens()
        {
            var loginRequest = new
            {
                email = "testuser@example.com",
                password = "TestPassword123!",
            };

            var response = await Client!.PostAsJsonAsync("/auth/login", loginRequest);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            doc.RootElement.TryGetProperty("isSuccess", out var isSuccessProp).Should().BeTrue();
            if (!isSuccessProp.GetBoolean())
            {
                // Вывести ошибку из ответа
                throw new Xunit.Sdk.XunitException($"Login failed. Response: {content}");
            }
            doc.RootElement.TryGetProperty("value", out var valueProp).Should().BeTrue();
            valueProp.ValueKind.Should().Be(JsonValueKind.Object);
            valueProp.TryGetProperty("accessToken", out var accessTokenProp).Should().BeTrue();
            accessTokenProp.GetString().Should().NotBeNullOrWhiteSpace();
            valueProp.TryGetProperty("refreshToken", out var refreshTokenProp).Should().BeTrue();
            refreshTokenProp.GetString().Should().NotBeNullOrWhiteSpace();
        }
    }
}
