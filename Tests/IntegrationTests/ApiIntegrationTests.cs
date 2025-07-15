using DotNetCleanTemplate.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests
{
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task RegisterUserEndpoint_ShouldReturnSuccess()
        {
            var response = await _client.PostAsJsonAsync(
                "/auth/register",
                new
                {
                    UserName = "testuser",
                    Email = "test@example.com",
                    Password = "Test123!",
                }
            );
            Assert.True(
                response.StatusCode == HttpStatusCode.OK
                    || response.StatusCode == HttpStatusCode.Created
            );
        }

        [Fact]
        public async Task LoginEndpoint_ShouldReturnToken()
        {
            var response = await _client.PostAsJsonAsync(
                "/auth/login",
                new { Email = "test@example.com", Password = "Test123!" }
            );
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // Можно добавить проверку на наличие токена в ответе
        }

        [Fact]
        public async Task RefreshTokenEndpoint_ShouldReturnNewToken()
        {
            // Предполагается, что refreshToken получен заранее
            var response = await _client.PostAsJsonAsync(
                "/auth/refresh",
                new { RefreshToken = "some-token" }
            );
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAllUsersWithRolesEndpoint_ShouldReturnUsers()
        {
            var response = await _client.GetAsync("/users");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task HelloEndpoint_ShouldReturnHello()
        {
            var response = await _client.GetAsync("/hello");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ErrorHandlingMiddleware_ShouldHandleException()
        {
            var response = await _client.GetAsync("/throw-error");
            Assert.True(
                response.StatusCode == HttpStatusCode.InternalServerError
                    || response.StatusCode == HttpStatusCode.BadRequest
            );
        }

        [Fact]
        public async Task CorsExtension_ShouldAllowCors()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, "/hello");
            request.Headers.Add("Origin", "http://localhost");
            request.Headers.Add("Access-Control-Request-Method", "GET");
            var response = await _client.SendAsync(request);
            Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        }
    }
}
