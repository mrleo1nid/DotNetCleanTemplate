using System.Net.Http.Json;
using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.Infrastructure.Configurations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace IntegrationTests
{
    public class TestBase : IAsyncLifetime, IClassFixture<CustomWebApplicationFactory<Program>>
    {
        protected CustomWebApplicationFactory<Program> Factory { get; private set; }
        protected HttpClient Client { get; private set; } = null!;

        public TestBase(CustomWebApplicationFactory<Program> factory)
        {
            Factory = factory;
        }

        public async Task InitializeAsync()
        {
            await Factory.InitializeAsync();
            Client = Factory.CreateClient();
            Console.WriteLine(
                $"[TestBase] Postgres connection string: {Factory.PostgresContainer.GetConnectionString()}"
            );
        }

        public Task DisposeAsync()
        {
            Client?.Dispose();
            return Task.CompletedTask;
        }

        public async Task<string> AuthenticateAsync(string email, string password)
        {
            var loginResponse = await Client.PostAsJsonAsync(
                "/auth/login",
                new { Email = email, Password = password }
            );
            loginResponse.EnsureSuccessStatusCode();
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var accessToken = System
                .Text.Json.JsonDocument.Parse(loginContent)
                .RootElement.GetProperty("value")
                .GetProperty("accessToken")
                .GetString();
            return accessToken!;
        }
    }
}
