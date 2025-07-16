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
                builder.UseSetting(
                    "ConnectionStrings:DefaultConnection",
                    PostgresContainer.GetConnectionString()
                );

                builder.ConfigureAppConfiguration(
                    (context, configBuilder) =>
                    {
                        configBuilder.Sources.Clear(); // <-- очень важно, очищает дефолтные источники

                        var testSettings = new Dictionary<string, string>
                        {
                            ["ConnectionStrings:DefaultConnection"] =
                                PostgresContainer.GetConnectionString(),

                            ["InitData:Roles:0:Name"] = "TestRole",
                            ["InitData:Users:0:UserName"] = "testuser",
                            ["InitData:Users:0:Email"] = "testuser@example.com",
                            ["InitData:Users:0:Password"] = "TestPassword123!",
                            ["InitData:Users:0:Roles:0"] = "TestRole",

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

                        configBuilder.AddInMemoryCollection(testSettings);
                    }
                );
            });
            // Перестроим конфигурацию поверх factory.Services
            var config = Factory.Services.GetRequiredService<IConfiguration>();
            _output.WriteLine("[Config dump after Factory]:");
            foreach (var kv in config.AsEnumerable())
            {
                _output.WriteLine($"[Config] {kv.Key} = {kv.Value}");
            }
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
