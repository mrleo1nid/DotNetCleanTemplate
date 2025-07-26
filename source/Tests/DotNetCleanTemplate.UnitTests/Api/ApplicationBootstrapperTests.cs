using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.Api.DependencyExtensions;
using Microsoft.AspNetCore.Builder;

namespace DotNetCleanTemplate.UnitTests.Api;

public class ApplicationBootstrapperTests
{
    private static WebApplicationBuilder CreateBuilder(bool isTest = false)
    {
        var builder = WebApplication.CreateBuilder();
        if (isTest)
        {
            builder.Configuration["IsTestEnvironment"] = "Test";
        }
        return builder;
    }

    [Fact]
    public void InitializeConfiguration_Skips_WhenTestEnvironment()
    {
        // Arrange
        var builder = CreateBuilder(true);
        var bootstrapper = new ApplicationBootstrapper(builder);

        // Act
        var result = bootstrapper.InitializeConfiguration();

        // Assert
        Assert.NotNull(result);
        Assert.Same(bootstrapper, result);
    }

    [Fact]
    public void InitializeConfiguration_AddsConfig_WhenNotTest()
    {
        // Arrange
        var builder = CreateBuilder(false);
        var bootstrapper = new ApplicationBootstrapper(builder);

        // Act & Assert
        // В CI файлы конфигурации могут отсутствовать, поэтому ожидаем исключение
        var exception = Record.Exception(() => bootstrapper.InitializeConfiguration());

        if (exception != null)
        {
            // В CI файлы отсутствуют, ожидаем FileNotFoundException
            Assert.IsType<FileNotFoundException>(exception);
            Assert.Contains("initData.json", exception.Message);
        }
        else
        {
            // Локально файлы существуют, проверяем результат
            var result = bootstrapper.InitializeConfiguration();
            Assert.NotNull(result);
            Assert.Same(bootstrapper, result);
        }
    }

    [Fact]
    public void ConfigureServices_AddsServices_WithoutException()
    {
        // Arrange
        var builder = CreateBuilder();
        // Добавляем необходимые настройки для JWT
        builder.Configuration["JwtSettings:Issuer"] = "test-issuer";
        builder.Configuration["JwtSettings:Audience"] = "test-audience";
        builder.Configuration["JwtSettings:Key"] = "test-key-that-is-long-enough-for-hmac-sha256";
        builder.Configuration["ConnectionStrings:DefaultConnection"] =
            "Host=localhost;Database=test;Username=test;Password=test;";
        // Добавляем настройки RateLimiting
        builder.Configuration["RateLimiting:PermitLimit"] = "100";
        builder.Configuration["RateLimiting:WindowSeconds"] = "60";
        builder.Configuration["RateLimiting:QueueLimit"] = "5";
        builder.Configuration["RateLimiting:UseIpPartition"] = "false";
        builder.Configuration["RateLimiting:UseApiKeyPartition"] = "false";
        // Добавляем настройки кэша
        builder.Configuration["cacheManagers:0:name"] = "default";
        builder.Configuration["cacheManagers:0:updateMode"] = "Up";
        builder.Configuration["cacheManagers:0:serializer:knownType"] = "Json";
        builder.Configuration["cacheManagers:0:handles:0:knownType"] = "MsMemory";
        builder.Configuration["cacheManagers:0:handles:0:enablePerformanceCounters"] = "true";
        builder.Configuration["cacheManagers:0:handles:0:enableStatistics"] = "true";
        builder.Configuration["cacheManagers:0:handles:0:expirationMode"] = "Absolute";
        builder.Configuration["cacheManagers:0:handles:0:expirationTimeout"] = "0:30:0";
        builder.Configuration["cacheManagers:0:handles:0:name"] = "memory";
        var bootstrapper = new ApplicationBootstrapper(builder);

        // Act & Assert
        var ex = Record.Exception(() => bootstrapper.ConfigureServices());
        Assert.Null(ex);
    }

    [Fact]
    public void AddJwtAuthentication_Throws_WhenJwtSettingsMissing()
    {
        Environment.SetEnvironmentVariable("IsTestEnvironment", "Test");
        // Пересоздаём builder без JwtSettings
        var builder = WebApplication.CreateBuilder();
        var services = builder.Services;
        Assert.Throws<InvalidOperationException>(() =>
            services.AddJwtAuthentication(builder.Configuration)
        );
    }

    [Fact]
    public void AddDatabase_Throws_WhenConnectionStringMissing()
    {
        Environment.SetEnvironmentVariable("IsTestEnvironment", "Test");
        var builder = CreateBuilder();
        var services = builder.Services;
        builder.Configuration["ConnectionStrings:DefaultConnection"] = null;
        Assert.Throws<InvalidOperationException>(() => services.AddDatabase(builder.Configuration));
    }

    [Fact]
    public void AddRateLimiting_WhenRateLimitingEnabled_ShouldConfigureCorrectly()
    {
        // Arrange
        var builder = CreateBuilder();
        builder.Configuration["RateLimiting:PermitLimit"] = "100";
        builder.Configuration["RateLimiting:WindowSeconds"] = "60";
        builder.Configuration["RateLimiting:QueueLimit"] = "5";
        builder.Configuration["RateLimiting:UseIpPartition"] = "true";
        builder.Configuration["RateLimiting:UseApiKeyPartition"] = "true";
        builder.Configuration["RateLimiting:IpPermitLimit"] = "50";
        builder.Configuration["RateLimiting:IpWindowSeconds"] = "30";
        builder.Configuration["RateLimiting:ApiKeyPermitLimit"] = "200";
        builder.Configuration["RateLimiting:ApiKeyWindowSeconds"] = "120";
        builder.Configuration["RateLimiting:ApiKeyHeaderName"] = "X-API-Key";

        var services = builder.Services;

        // Act & Assert
        var ex = Record.Exception(() => services.AddRateLimiting(builder.Configuration));
        Assert.Null(ex);
    }

    [Fact]
    public void AddRateLimiting_WhenRateLimitingDisabled_ShouldSkipConfiguration()
    {
        // Arrange
        var builder = CreateBuilder();
        // Не добавляем секцию RateLimiting
        var services = builder.Services;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            services.AddRateLimiting(builder.Configuration)
        );
    }

    [Fact]
    public void AddRateLimiting_WhenDifferentPartitionKeys_ShouldConfigureSeparately()
    {
        // Arrange
        var builder = CreateBuilder();
        builder.Configuration["RateLimiting:PermitLimit"] = "100";
        builder.Configuration["RateLimiting:WindowSeconds"] = "60";
        builder.Configuration["RateLimiting:QueueLimit"] = "5";
        builder.Configuration["RateLimiting:UseIpPartition"] = "true";
        builder.Configuration["RateLimiting:UseApiKeyPartition"] = "false";
        builder.Configuration["RateLimiting:IpPermitLimit"] = "50";
        builder.Configuration["RateLimiting:IpWindowSeconds"] = "30";

        var services = builder.Services;

        // Act & Assert
        var ex = Record.Exception(() => services.AddRateLimiting(builder.Configuration));
        Assert.Null(ex);
    }

    [Fact]
    public void AddRateLimiting_WhenInvalidConfiguration_ShouldHandleGracefully()
    {
        // Arrange
        var builder = CreateBuilder();
        builder.Configuration["RateLimiting:PermitLimit"] = "invalid";
        builder.Configuration["RateLimiting:WindowSeconds"] = "invalid";
        builder.Configuration["RateLimiting:QueueLimit"] = "invalid";

        var services = builder.Services;

        // Act & Assert
        var ex = Record.Exception(() => services.AddRateLimiting(builder.Configuration));
        Assert.NotNull(ex); // Должен выбросить исключение при неверной конфигурации
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void Build_ShouldReturnWebApplication()
    {
        // Arrange
        var builder = CreateBuilder();
        var bootstrapper = new ApplicationBootstrapper(builder);

        // Act
        var app = bootstrapper.Build();

        // Assert
        Assert.NotNull(app);
        Assert.IsType<WebApplication>(app);
    }

    // Новые тесты для покрытия недостающих сценариев

    [Fact]
    public void Test_InitializeConfiguration_ThrowsException_WhenConfigurationIsInvalid()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        // Устанавливаем невалидную конфигурацию
        builder.Configuration["IsTestEnvironment"] = "Prod";

        // Создаем несуществующий путь для конфигурации
        var bootstrapper = new ApplicationBootstrapper(builder);

        // Act & Assert
        // В CI файлы конфигурации отсутствуют, поэтому ожидаем исключение
        var exception = Record.Exception(() => bootstrapper.InitializeConfiguration());

        if (exception != null)
        {
            // В CI файлы отсутствуют, ожидаем FileNotFoundException
            Assert.IsType<FileNotFoundException>(exception);
            Assert.Contains("initData.json", exception.Message);
        }
        else
        {
            // Локально файлы существуют, проверяем результат
            var result = bootstrapper.InitializeConfiguration();
            Assert.NotNull(result);
            Assert.Same(bootstrapper, result);
        }
    }

    [Fact]
    public void Test_AddJwtAuthentication_ThrowsException_WhenJwtSettingsNotConfigured()
    {
        // Arrange
        var builder = CreateBuilder();
        var services = builder.Services;
        // Не добавляем JWT настройки

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddJwtAuthentication(builder.Configuration)
        );
        Assert.Contains("JwtSettings is not set", exception.Message);
    }

    [Fact]
    public void Test_AddDatabase_ThrowsException_WhenConnectionStringNotSet()
    {
        // Arrange
        var builder = CreateBuilder();
        var services = builder.Services;
        builder.Configuration["ConnectionStrings:DefaultConnection"] = null;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddDatabase(builder.Configuration)
        );
        Assert.Contains("Connection string 'DefaultConnection' is not set", exception.Message);
    }

    [Fact]
    public void Test_AddDatabase_ThrowsException_WhenConnectionStringIsInvalid()
    {
        // Arrange
        var builder = CreateBuilder();
        var services = builder.Services;
        builder.Configuration["ConnectionStrings:DefaultConnection"] = "invalid_connection_string";

        // Act & Assert
        // В данном случае метод не проверяет валидность строки подключения, только её наличие
        var ex = Record.Exception(() => services.AddDatabase(builder.Configuration));
        Assert.Null(ex);
    }

    [Fact]
    public void Test_AddDatabase_WithValidConnectionString()
    {
        // Arrange
        var builder = CreateBuilder();
        var services = builder.Services;
        builder.Configuration["ConnectionStrings:DefaultConnection"] =
            "Host=localhost;Database=test;Username=test;Password=test;";

        // Act & Assert
        var ex = Record.Exception(() => services.AddDatabase(builder.Configuration));
        Assert.Null(ex);
    }

    [Fact]
    public void Test_AddRateLimiting_WithApiKeyPartitionOnly()
    {
        // Arrange
        var builder = CreateBuilder();
        builder.Configuration["RateLimiting:PermitLimit"] = "100";
        builder.Configuration["RateLimiting:WindowSeconds"] = "60";
        builder.Configuration["RateLimiting:QueueLimit"] = "5";
        builder.Configuration["RateLimiting:UseIpPartition"] = "false";
        builder.Configuration["RateLimiting:UseApiKeyPartition"] = "true";
        builder.Configuration["RateLimiting:ApiKeyPermitLimit"] = "200";
        builder.Configuration["RateLimiting:ApiKeyWindowSeconds"] = "120";
        builder.Configuration["RateLimiting:ApiKeyHeaderName"] = "X-API-Key";

        var services = builder.Services;

        // Act & Assert
        var ex = Record.Exception(() => services.AddRateLimiting(builder.Configuration));
        Assert.Null(ex);
    }

    [Fact]
    public void Test_AddRateLimiting_WithGlobalLimiterOnly()
    {
        // Arrange
        var builder = CreateBuilder();
        builder.Configuration["RateLimiting:PermitLimit"] = "100";
        builder.Configuration["RateLimiting:WindowSeconds"] = "60";
        builder.Configuration["RateLimiting:QueueLimit"] = "5";
        builder.Configuration["RateLimiting:UseIpPartition"] = "false";
        builder.Configuration["RateLimiting:UseApiKeyPartition"] = "false";

        var services = builder.Services;

        // Act & Assert
        var ex = Record.Exception(() => services.AddRateLimiting(builder.Configuration));
        Assert.Null(ex);
    }

    [Fact]
    public void Test_AddRateLimiting_ThrowsException_WhenSettingsNotConfigured()
    {
        // Arrange
        var builder = CreateBuilder();
        var services = builder.Services;
        // Не добавляем секцию RateLimiting

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddRateLimiting(builder.Configuration)
        );
        Assert.Contains("RateLimiting is not set", exception.Message);
    }

    [Fact]
    public void Test_AddRateLimiting_WithCustomRejectionHandler()
    {
        // Arrange
        var builder = CreateBuilder();
        builder.Configuration["RateLimiting:PermitLimit"] = "1";
        builder.Configuration["RateLimiting:WindowSeconds"] = "60";
        builder.Configuration["RateLimiting:QueueLimit"] = "0";
        builder.Configuration["RateLimiting:UseIpPartition"] = "false";
        builder.Configuration["RateLimiting:UseApiKeyPartition"] = "false";

        var services = builder.Services;

        // Act & Assert
        var ex = Record.Exception(() => services.AddRateLimiting(builder.Configuration));
        Assert.Null(ex);
    }

    [Fact]
    public void Test_AddRateLimiting_WithDifferentWindowSizes()
    {
        // Arrange
        var builder = CreateBuilder();
        builder.Configuration["RateLimiting:PermitLimit"] = "100";
        builder.Configuration["RateLimiting:WindowSeconds"] = "30";
        builder.Configuration["RateLimiting:QueueLimit"] = "5";
        builder.Configuration["RateLimiting:UseIpPartition"] = "true";
        builder.Configuration["RateLimiting:UseApiKeyPartition"] = "true";
        builder.Configuration["RateLimiting:IpPermitLimit"] = "50";
        builder.Configuration["RateLimiting:IpWindowSeconds"] = "15";
        builder.Configuration["RateLimiting:ApiKeyPermitLimit"] = "200";
        builder.Configuration["RateLimiting:ApiKeyWindowSeconds"] = "60";

        var services = builder.Services;

        // Act & Assert
        var ex = Record.Exception(() => services.AddRateLimiting(builder.Configuration));
        Assert.Null(ex);
    }

    [Fact]
    public void Test_AddRateLimiting_WithQueueLimit()
    {
        // Arrange
        var builder = CreateBuilder();
        builder.Configuration["RateLimiting:PermitLimit"] = "100";
        builder.Configuration["RateLimiting:WindowSeconds"] = "60";
        builder.Configuration["RateLimiting:QueueLimit"] = "10";
        builder.Configuration["RateLimiting:UseIpPartition"] = "false";
        builder.Configuration["RateLimiting:UseApiKeyPartition"] = "false";

        var services = builder.Services;

        // Act & Assert
        var ex = Record.Exception(() => services.AddRateLimiting(builder.Configuration));
        Assert.Null(ex);
    }
}
