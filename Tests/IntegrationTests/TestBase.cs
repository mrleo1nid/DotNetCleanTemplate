using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.Infrastructure.Configurations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class TestBase : IAsyncLifetime
    {
        protected RedisContainer RedisContainer { get; private set; }
        protected PostgreSqlContainer PostgresContainer { get; private set; }
        protected WebApplicationFactory<Program> Factory { get; private set; } = null!;
        protected HttpClient Client { get; private set; } = null!;
        private readonly ITestOutputHelper _output;

        public TestBase(ITestOutputHelper output)
        {
            _output = output;
            RedisContainer = new RedisBuilder()
                .WithImage("redis:7.2-alpine")
                .WithPortBinding(6379, true)
                .Build();

            PostgresContainer = new PostgreSqlBuilder()
                .WithDatabase("testdb")
                .WithUsername("testuser")
                .WithPassword("testpass")
                .WithCleanUp(true)
                .Build();
        }

        public async Task InitializeAsync()
        {
            await RedisContainer.StartAsync();
            await PostgresContainer.StartAsync();
            var redisConnectionString = RedisContainer.GetConnectionString();
            _output.WriteLine($"[TestBase] Redis connection string: {redisConnectionString}");

            Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    (context, config) =>
                    {
                        var testSettings = new Dictionary<string, string>
                        {
                            ["ConnectionStrings:DefaultConnection"] =
                                PostgresContainer.GetConnectionString(),

                            // InitData
                            ["InitData:Roles:0:Name"] = "TestRole",
                            ["InitData:Users:0:UserName"] = "testuser",
                            ["InitData:Users:0:Email"] = "testuser@example.com",
                            ["InitData:Users:0:Password"] = "TestPassword123!",
                            ["InitData:Users:0:Roles:0"] = "TestRole",

                            // Redis config (эквивалентно cache.json)
                            ["redis:0:key"] = "redisConnection",
                            ["redis:0:connectionString"] = redisConnectionString,

                            ["cacheManagers:0:name"] = "default",
                            ["cacheManagers:0:updateMode"] = "Up",
                            ["cacheManagers:0:serializer:knownType"] = "Json",

                            ["cacheManagers:0:handles:0:knownType"] = "MsMemory",
                            ["cacheManagers:0:handles:0:enablePerformanceCounters"] = "true",
                            ["cacheManagers:0:handles:0:enableStatistics"] = "true",
                            ["cacheManagers:0:handles:0:expirationMode"] = "Absolute",
                            ["cacheManagers:0:handles:0:expirationTimeout"] = "0:30:0",
                            ["cacheManagers:0:handles:0:name"] = "memory",

                            ["cacheManagers:0:handles:1:knownType"] = "Redis",
                            ["cacheManagers:0:handles:1:key"] = "redisConnection",
                            ["cacheManagers:0:handles:1:isBackplaneSource"] = "true",
                            ["cacheManagers:0:handles:1:name"] = "redis",
                        };

                        config.AddInMemoryCollection(testSettings!);

                        // Вывод всей конфигурации для отладки
                        var builtConfig = config.Build();
                        _output.WriteLine("[Config dump]:");
                        foreach (var kv in builtConfig.AsEnumerable())
                        {
                            _output.WriteLine($"[Config] {kv.Key} = {kv.Value}");
                        }
                    }
                );
            });
            _output.WriteLine($"[TestBase] Factory created");
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
