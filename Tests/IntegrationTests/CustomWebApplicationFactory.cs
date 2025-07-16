using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace IntegrationTests
{
    public class CustomWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
        where TEntryPoint : class
    {
        public readonly PostgreSqlContainer PostgresContainer;

        public CustomWebApplicationFactory()
        {
            Environment.SetEnvironmentVariable("IsTestEnvironment", "Test");
            PostgresContainer = new PostgreSqlBuilder()
                .WithDatabase("testdb")
                .WithUsername("testuser")
                .WithPassword("testpass")
                .WithCleanUp(true)
                .Build();
        }

        public async Task InitializeAsync()
        {
            await PostgresContainer.StartAsync();
        }

        public override async ValueTask DisposeAsync()
        {
            await PostgresContainer.DisposeAsync();
        }
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureHostConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?> {
                    ["ConnectionStrings:DefaultConnection"] =
                                PostgresContainer.GetConnectionString(),

                    ["InitData:Roles:0:Name"] = "TestRole",
                    ["InitData:Users:0:UserName"] = "testuser",
                    ["InitData:Users:0:Email"] = "testuser@example.com",
                    ["InitData:Users:0:Password"] = "TestPassword123!",
                    ["InitData:Users:0:Roles:0"] = "TestRole",

                    ["cacheManagers:0:name"] = "default",
                    ["cacheManagers:0:updateMode"] = "Up",
                    ["cacheManagers:0:serializer:knownType"] = "Json",

                    ["cacheManagers:0:handles:0:knownType"] = "MsMemory",
                    ["cacheManagers:0:handles:0:enablePerformanceCounters"] = "true",
                    ["cacheManagers:0:handles:0:enableStatistics"] = "true",
                    ["cacheManagers:0:handles:0:expirationMode"] = "Absolute",
                    ["cacheManagers:0:handles:0:expirationTimeout"] = "0:30:0",
                    ["cacheManagers:0:handles:0:name"] = "memory",

                    ["DatabaseSettings:ApplyMigrationsOnStartup"] = "true",

                    ["JwtSettings:Key"] = "testkeytestkeytestkeytestkeytestkey111111",
                    ["JwtSettings:Issuer"] = "testissuer",
                    ["JwtSettings:Audience"] = "testaudience",
                    ["JwtSettings:AccessTokenExpirationMinutes"] = "30",
                    ["JwtSettings:RefreshTokenExpirationDays"] = "7",

                    ["redis:0:key"] = "redisConnection",
                    ["redis:0:connectionString"] = "localhost",

                    ["Cors:AllowedOrigins:0"] = "http://localhost:3000",
                    ["Cors:AllowedOrigins:1"] = "http://localhost:5173",
                    ["Cors:AllowedOrigins:2"] = "http://localhost",
                });
            });
            return base.CreateHost(builder);
        }
    }
}
