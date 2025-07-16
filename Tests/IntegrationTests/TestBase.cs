using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.Infrastructure.Configurations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class TestBase : IAsyncLifetime, IClassFixture<CustomWebApplicationFactory<Program>>
    {
        protected CustomWebApplicationFactory<Program> Factory { get; private set; } = null!;
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
                $"[TestBase] Redis connection string: {Factory.RedisContainer.GetConnectionString()}"
            );
            Console.WriteLine(
                $"[TestBase] Postgres connection string: {Factory.PostgresContainer.GetConnectionString()}"
            );
        }

        protected void PrintConfiguration(IConfiguration config, string? parentPath = null)
        {
            foreach (var child in config.GetChildren())
            {
                var key = parentPath == null ? child.Key : $"{parentPath}:{child.Key}";
                if (child.GetChildren().Any())
                {
                    PrintConfiguration(child, key);
                }
                else
                {
                    Console.WriteLine($"{key} = {child.Value}");
                }
            }
        }

        public async Task DisposeAsync()
        {
            await Factory.DisposeAsync();
            Client?.Dispose();
        }
    }
}
