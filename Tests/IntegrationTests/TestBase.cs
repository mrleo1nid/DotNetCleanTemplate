using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.Infrastructure.Configurations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
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
                .WithCleanUp(true)
                .Build();
        }

        public async Task InitializeAsync()
        {
            await RedisContainer.StartAsync();
            await PostgresContainer.StartAsync();

            Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    (context, config) =>
                    {
                        var redisConnectionString = RedisContainer.GetConnectionString();
                        Console.WriteLine(
                            $"[TestBase] Redis connection string: {redisConnectionString}"
                        );

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
                        };
                        config.AddInMemoryCollection(testSettings!);

                        // Добавляем cache.json через AddJsonStream
                        var cacheJson =
                            $@"
{{
  ""redis"": [
    {{
      ""key"": ""redisConnection"",
      ""connectionString"": ""{redisConnectionString}""
    }}
  ],
  ""cacheManagers"": [
    {{
      ""name"": ""default"",
      ""updateMode"": ""Up"",
      ""serializer"": {{ ""knownType"": ""Json"" }},
      ""handles"": [
        {{
          ""knownType"": ""MsMemory"",
          ""enablePerformanceCounters"": true,
          ""enableStatistics"": true,
          ""expirationMode"": ""Absolute"",
          ""expirationTimeout"": ""0:30:0"",
          ""name"": ""memory""
        }},
        {{
          ""knownType"": ""Redis"",
          ""key"": ""redisConnection"",
          ""isBackplaneSource"": true,
          ""name"": ""redis""
        }}
      ]
    }}
  ]
}}";
                        using var cacheStream = new System.IO.MemoryStream(
                            System.Text.Encoding.UTF8.GetBytes(cacheJson)
                        );
                        config.AddJsonStream(cacheStream);
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
