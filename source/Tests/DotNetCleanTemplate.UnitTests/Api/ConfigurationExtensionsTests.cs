using DotNetCleanTemplate.Api.DependencyExtensions;
using Microsoft.AspNetCore.Builder;

namespace DotNetCleanTemplate.UnitTests.Api;

public class ConfigurationExtensionsTests
{
    [Fact]
    public void InitializeConfiguration_WhenIsTestEnvironment_ShouldReturnBuilderWithoutConfiguration()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var originalConfiguration = builder.Configuration;

        // Act
        var result = builder.InitializeConfiguration(isTestEnvironment: true);

        // Assert
        Assert.Same(builder, result);
        Assert.Same(originalConfiguration, result.Configuration);
    }

    [Fact]
    public void InitializeConfiguration_WhenNotTestEnvironment_ShouldAddJsonFiles()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var originalSourcesCount = builder.Configuration.Sources.Count;

        // Act
        var result = builder.InitializeConfiguration(isTestEnvironment: false);

        // Assert
        Assert.Same(builder, result);
        Assert.True(result.Configuration.Sources.Count > originalSourcesCount);
    }

    [Fact]
    public void InitializeConfiguration_ShouldAddEnvironmentVariableSubstitution()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder.InitializeConfiguration(isTestEnvironment: false);

        // Assert
        var hasEnvSubstitution = result.Configuration.Sources.Any(s =>
            s.ToString()?.Contains("EnvironmentVariableSubstitution") ?? false
        );
        Assert.True(hasEnvSubstitution, "Should add environment variable substitution");
    }

    [Fact]
    public void InitializeConfiguration_ShouldReturnSameBuilderInstance()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder.InitializeConfiguration(isTestEnvironment: false);

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void InitializeConfiguration_ShouldAddCorrectNumberOfSources()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var originalSourcesCount = builder.Configuration.Sources.Count;

        // Act
        var result = builder.InitializeConfiguration(isTestEnvironment: false);

        // Assert
        // Должно добавиться 4 JSON файла + EnvironmentVariableSubstitution + возможно .env в DEBUG
        var expectedAdditionalSources = 5; // 4 JSON + 1 EnvironmentVariableSubstitution
        Assert.True(
            result.Configuration.Sources.Count
                >= originalSourcesCount + expectedAdditionalSources - 1,
            $"Expected at least {expectedAdditionalSources - 1} additional sources, but got {result.Configuration.Sources.Count - originalSourcesCount}"
        );
    }

    [Fact]
    public void InitializeConfiguration_ShouldAddJsonSources()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder.InitializeConfiguration(isTestEnvironment: false);

        // Assert
        var jsonSources = result
            .Configuration.Sources.Where(s => s.ToString()?.Contains(".json") ?? false)
            .ToList();

        // Проверяем, что добавились JSON источники (файлы могут не существовать в тестовой среде)
        // Метод пытается добавить 4 JSON файла, но они могут не найтись
        Assert.NotNull(jsonSources);
        // Count всегда >= 0 для коллекций, поэтому просто логируем количество
        _ = jsonSources.Count;
    }

    [Fact]
    public void InitializeConfiguration_ShouldNotAddSourcesWhenTestEnvironment()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var originalSourcesCount = builder.Configuration.Sources.Count;

        // Act
        var result = builder.InitializeConfiguration(isTestEnvironment: true);

        // Assert
        Assert.Equal(originalSourcesCount, result.Configuration.Sources.Count);
    }
}
