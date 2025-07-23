using DotNetCleanTemplate.WebClient.Configurations;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.WebClient.Configurations;

public class ClientConfigTests
{
    private const string TestBaseUrl = "http://localhost";

    [Fact]
    public void ClientConfig_WhenCreated_HasDefaultApiSection()
    {
        // Act
        var config = new ClientConfig();

        // Assert
        Assert.NotNull(config.Api);
        Assert.IsType<ApiSection>(config.Api);
    }

    [Fact]
    public void ApiSection_WhenCreated_HasDefaultValues()
    {
        // Act
        var apiSection = new ApiSection();

        // Assert
        Assert.Equal(string.Empty, apiSection.BaseUrl);
    }

    [Fact]
    public void ApiSection_SectionName_IsCorrect()
    {
        // Assert
        Assert.Equal("Api", ApiSection.SectionName);
    }

    [Fact]
    public void ClientConfig_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var config = new ClientConfig();
        var apiSection = new ApiSection { BaseUrl = TestBaseUrl };

        // Act
        config.Api = apiSection;

        // Assert
        Assert.Equal(TestBaseUrl, config.Api.BaseUrl);
    }

    [Fact]
    public void ApiSection_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var apiSection = new ApiSection();

        // Act
        apiSection.BaseUrl = TestBaseUrl;

        // Assert
        Assert.Equal(TestBaseUrl, apiSection.BaseUrl);
    }

    [Fact]
    public void ClientConfig_WhenApiIsNull_ThrowsNoException()
    {
        // Act & Assert
        var config = new ClientConfig();
        config.Api = null!;

        // Не должно выбрасывать исключение при установке null
        Assert.Null(config.Api);
    }
}
