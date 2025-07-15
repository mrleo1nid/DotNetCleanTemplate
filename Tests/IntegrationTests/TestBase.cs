using DotNetCleanTemplate.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace IntegrationTests
{
    public class TestBase : IAsyncLifetime
    {
        protected RedisContainer RedisContainer { get; private set; }
        protected PostgreSqlContainer PostgresContainer { get; private set; }
        protected WebApplicationFactory<Program> Factory { get; private set; } = null!;
        protected HttpClient Client { get; private set; } = null!;

        public TestBase()
        {
            RedisContainer = new RedisBuilder()
                .WithImage("redis:7.2-alpine")
                .WithPortBinding(6379, true)
                .Build();

            PostgresContainer = new PostgreSqlBuilder()
                .WithDatabase("testdb")
                .WithUsername("testuser")
                .WithPassword("testpass")
                .Build();
        }

        public async Task InitializeAsync()
        {
            await RedisContainer.StartAsync();
            await PostgresContainer.StartAsync();

            Environment.SetEnvironmentVariable(
                "REDIS_CONNECTION_STRING",
                RedisContainer.GetConnectionString()
            );
            Environment.SetEnvironmentVariable(
                "ConnectionStrings__DefaultConnection",
                PostgresContainer.GetConnectionString()
            );

            Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    (context, config) =>
                    {
                        // Можно добавить дополнительные настройки, если потребуется
                    }
                );
            });

            Client = Factory.CreateClient();
        }

        public async Task DisposeAsync()
        {
            await RedisContainer.DisposeAsync();
            await PostgresContainer.DisposeAsync();
            Factory?.Dispose();
            Client?.Dispose();
        }
    }
}
