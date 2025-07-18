using DotNetCleanTemplate.Api.Helpers;
using DotNetCleanTemplate.Infrastructure.Configurations;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace DotNetCleanTemplate.ApplicationTests;

public class RateLimitingPartitionLogicTests
{
    [Fact]
    public void GetPartitionKey_WithApiKey_ShouldReturnApiKeyPartition()
    {
        // Arrange
        var settings = new RateLimitingSettings
        {
            UseApiKeyPartition = true,
            UseIpPartition = true,
            ApiKeyHeaderName = "X-API-Key",
        };

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-API-Key"] = "test-api-key";

        // Act
        var partitionKey = HttpContextHelper.GetPartitionKey(httpContext, settings);

        // Assert
        Assert.Equal("api_key:test-api-key", partitionKey);
    }

    [Fact]
    public void GetPartitionKey_WithoutApiKey_ShouldReturnIpPartition()
    {
        // Arrange
        var settings = new RateLimitingSettings
        {
            UseApiKeyPartition = true,
            UseIpPartition = true,
            ApiKeyHeaderName = "X-API-Key",
        };

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        // Act
        var partitionKey = HttpContextHelper.GetPartitionKey(httpContext, settings);

        // Assert
        Assert.Equal("ip:127.0.0.1", partitionKey);
    }

    [Fact]
    public void GetPartitionKey_WithXForwardedFor_ShouldReturnFirstIp()
    {
        // Arrange
        var settings = new RateLimitingSettings
        {
            UseApiKeyPartition = false,
            UseIpPartition = true,
        };

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Forwarded-For"] = "192.168.1.1, 10.0.0.1, 172.16.0.1";

        // Act
        var partitionKey = HttpContextHelper.GetPartitionKey(httpContext, settings);

        // Assert
        Assert.Equal("ip:192.168.1.1", partitionKey);
    }

    [Fact]
    public void GetPartitionKey_WithXRealIp_ShouldReturnRealIp()
    {
        // Arrange
        var settings = new RateLimitingSettings
        {
            UseApiKeyPartition = false,
            UseIpPartition = true,
        };

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Real-IP"] = "203.0.113.1";

        // Act
        var partitionKey = HttpContextHelper.GetPartitionKey(httpContext, settings);

        // Assert
        Assert.Equal("ip:203.0.113.1", partitionKey);
    }

    [Fact]
    public void GetPartitionKey_WithNoPartitionsEnabled_ShouldReturnGlobal()
    {
        // Arrange
        var settings = new RateLimitingSettings
        {
            UseApiKeyPartition = false,
            UseIpPartition = false,
        };

        var httpContext = new DefaultHttpContext();

        // Act
        var partitionKey = HttpContextHelper.GetPartitionKey(httpContext, settings);

        // Assert
        Assert.Equal("global", partitionKey);
    }

    [Fact]
    public void GetPartitionKey_WithEmptyApiKey_ShouldFallbackToIp()
    {
        // Arrange
        var settings = new RateLimitingSettings
        {
            UseApiKeyPartition = true,
            UseIpPartition = true,
            ApiKeyHeaderName = "X-API-Key",
        };

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-API-Key"] = "";
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        // Act
        var partitionKey = HttpContextHelper.GetPartitionKey(httpContext, settings);

        // Assert
        Assert.Equal("ip:127.0.0.1", partitionKey);
    }
}
