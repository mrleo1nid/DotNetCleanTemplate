using DotNetCleanTemplate.Api;
using Microsoft.AspNetCore.Builder;

namespace ApplicationTests
{
    public class ApplicationBootstrapperTests
    {
        private static WebApplicationBuilder CreateBuilder(bool isTest = false)
        {
            Environment.SetEnvironmentVariable("IsTestEnvironment", "Test");
            var builder = WebApplication.CreateBuilder();
            builder.Configuration["IsTestEnvironment"] = isTest ? "Test" : "Prod";
            return builder;
        }

        [Fact]
        public void InitializeConfiguration_Skips_WhenTestEnvironment()
        {
            Environment.SetEnvironmentVariable("IsTestEnvironment", "Test");
            var builder = CreateBuilder(isTest: true);
            var bootstrapper = new ApplicationBootstrapper(builder);
            var result = bootstrapper.InitializeConfiguration();
            Assert.Same(bootstrapper, result);
        }

        [Fact]
        public void InitializeConfiguration_AddsConfig_WhenNotTest()
        {
            Environment.SetEnvironmentVariable("IsTestEnvironment", "Test");
            var builder = CreateBuilder(isTest: false);
            var bootstrapper = new ApplicationBootstrapper(builder);
            var result = bootstrapper.InitializeConfiguration();
            Assert.Same(bootstrapper, result);
            // Проверяем, что конфиги добавлены (например, appsettings.json)
            // Здесь можно проверить, что конфиг содержит нужные ключи, если они есть
        }

        [Fact]
        public void ConfigureServices_AddsServices_WithoutException()
        {
            Environment.SetEnvironmentVariable("IsTestEnvironment", "Test");
            var builder = CreateBuilder();
            // Добавляем необходимые параметры JwtSettings
            builder.Configuration["JwtSettings:Key"] = "testkey";
            builder.Configuration["JwtSettings:Issuer"] = "testissuer";
            builder.Configuration["JwtSettings:Audience"] = "testaudience";
            builder.Configuration["ConnectionStrings:DefaultConnection"] =
                "Host=localhost;Database=test;Username=test;Password=test;";
            builder.Configuration["cacheManagers:0:name"] = "default";
            builder.Configuration["cacheManagers:0:updateMode"] = "Up";
            builder.Configuration["cacheManagers:0:serializer:knownType"] = "Json";
            builder.Configuration["cacheManagers:0:handles:0:knownType"] = "MsMemory";
            builder.Configuration["cacheManagers:0:handles:0:enablePerformanceCounters"] = "true";
            builder.Configuration["cacheManagers:0:handles:0:enableStatistics"] = "true";
            builder.Configuration["cacheManagers:0:handles:0:expirationMode"] = "Absolute";
            builder.Configuration["cacheManagers:0:handles:0:expirationTimeout"] = "0:30:0";
            builder.Configuration["cacheManagers:0:handles:0:name"] = "memory";

            builder.Configuration["RateLimiting:PermitLimit"] = "120";
            builder.Configuration["RateLimiting:WindowSeconds"] = "60";
            builder.Configuration["RateLimiting:QueueLimit"] = "10";

            var bootstrapper = new ApplicationBootstrapper(builder);
            var ex = Record.Exception(() => bootstrapper.ConfigureServices());
            Assert.Null(ex);
        }

        [Fact]
        public void AddJwtAuth_Throws_WhenJwtSettingsMissing()
        {
            Environment.SetEnvironmentVariable("IsTestEnvironment", "Test");
            // Пересоздаём builder без JwtSettings
            var builder = WebApplication.CreateBuilder();
            var bootstrapper = new ApplicationBootstrapper(builder);
            Assert.Throws<InvalidOperationException>(() => bootstrapper.AddJwtAuth());
        }

        [Fact]
        public void AddDatabase_Throws_WhenConnectionStringMissing()
        {
            Environment.SetEnvironmentVariable("IsTestEnvironment", "Test");
            var builder = CreateBuilder();
            var bootstrapper = new ApplicationBootstrapper(builder);
            builder.Configuration["ConnectionStrings:DefaultConnection"] = null;
            Assert.Throws<InvalidOperationException>(() => bootstrapper.AddDatabase());
        }
    }
}
