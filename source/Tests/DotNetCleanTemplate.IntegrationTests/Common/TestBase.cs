using DotNetCleanTemplate.Api;
using System.Net.Http.Json;

namespace DotNetCleanTemplate.IntegrationTests.Common
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
            Client.Timeout = TimeSpan.FromSeconds(60);
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
