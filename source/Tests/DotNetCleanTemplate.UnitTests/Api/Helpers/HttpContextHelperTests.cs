using DotNetCleanTemplate.Api.Helpers;
using DotNetCleanTemplate.Infrastructure.Configurations;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace DotNetCleanTemplate.UnitTests.Api.Helpers;

public class HttpContextHelperTests
{
    #region Constants

    private const string ApiKeyHeaderName = "X-API-Key";
    private const string TestIpAddress = "192.168.1.1";
    private const string TestIpAddress2 = "192.168.1.100";
    private const string ExpectedIpPartition = "ip:192.168.1.1";
    private const string XForwardedForHeader = "X-Forwarded-For";
    private const string XRealIpHeader = "X-Real-IP";

    #endregion

    #region GetPartitionKey Tests

    [Fact]
    public void GetPartitionKey_WithApiKeyEnabled_ShouldReturnApiKeyPartition()
    {
        // Arrange
        var settings = new RateLimitingSettings
        {
            UseApiKeyPartition = true,
            ApiKeyHeaderName = ApiKeyHeaderName,
        };

        var httpContext = CreateHttpContext();
        httpContext.Request.Headers[ApiKeyHeaderName] = "test-api-key";

        // Act
        var result = HttpContextHelper.GetPartitionKey(httpContext, settings);

        // Assert
        Assert.Equal("api_key:test-api-key", result);
    }

    [Fact]
    public void GetPartitionKey_WithApiKeyEnabledButNoKey_ShouldReturnIpPartition()
    {
        // Arrange
        var settings = new RateLimitingSettings
        {
            UseApiKeyPartition = true,
            UseIpPartition = true,
            ApiKeyHeaderName = ApiKeyHeaderName,
        };

        var httpContext = CreateHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse(TestIpAddress);

        // Act
        var result = HttpContextHelper.GetPartitionKey(httpContext, settings);

        // Assert
        Assert.Equal(ExpectedIpPartition, result);
    }

    [Fact]
    public void GetPartitionKey_WithIpPartitionEnabled_ShouldReturnIpPartition()
    {
        // Arrange
        var settings = new RateLimitingSettings { UseIpPartition = true };

        var httpContext = CreateHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse(TestIpAddress);

        // Act
        var result = HttpContextHelper.GetPartitionKey(httpContext, settings);

        // Assert
        Assert.Equal(ExpectedIpPartition, result);
    }

    [Fact]
    public void GetPartitionKey_WithNoPartitionEnabled_ShouldReturnGlobal()
    {
        // Arrange
        var settings = new RateLimitingSettings
        {
            UseApiKeyPartition = false,
            UseIpPartition = false,
        };

        var httpContext = CreateHttpContext();

        // Act
        var result = HttpContextHelper.GetPartitionKey(httpContext, settings);

        // Assert
        Assert.Equal("global", result);
    }

    [Fact]
    public void GetPartitionKey_WithApiKeyAndIpDisabled_ShouldReturnGlobal()
    {
        // Arrange
        var settings = new RateLimitingSettings
        {
            UseApiKeyPartition = false,
            UseIpPartition = false,
        };

        var httpContext = CreateHttpContext();
        httpContext.Request.Headers[ApiKeyHeaderName] = "test-api-key";
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse(TestIpAddress);

        // Act
        var result = HttpContextHelper.GetPartitionKey(httpContext, settings);

        // Assert
        Assert.Equal("global", result);
    }

    [Fact]
    public void GetPartitionKey_WithApiKeyEnabledAndEmptyKey_ShouldReturnIpPartition()
    {
        // Arrange
        var settings = new RateLimitingSettings
        {
            UseApiKeyPartition = true,
            UseIpPartition = true,
            ApiKeyHeaderName = ApiKeyHeaderName,
        };

        var httpContext = CreateHttpContext();
        httpContext.Request.Headers[ApiKeyHeaderName] = "";
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse(TestIpAddress);

        // Act
        var result = HttpContextHelper.GetPartitionKey(httpContext, settings);

        // Assert
        Assert.Equal(ExpectedIpPartition, result);
    }

    [Fact]
    public void GetPartitionKey_WithApiKeyEnabledAndWhitespaceKey_ShouldReturnIpPartition()
    {
        // Arrange
        var settings = new RateLimitingSettings
        {
            UseApiKeyPartition = true,
            UseIpPartition = true,
            ApiKeyHeaderName = ApiKeyHeaderName,
        };

        var httpContext = CreateHttpContext();
        httpContext.Request.Headers[ApiKeyHeaderName] = "   ";
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse(TestIpAddress);

        // Act
        var result = HttpContextHelper.GetPartitionKey(httpContext, settings);

        // Assert
        Assert.Equal(ExpectedIpPartition, result);
    }

    #endregion

    #region GetClientIpAddress Tests

    [Fact]
    public void GetClientIpAddress_WithXForwardedFor_ShouldReturnFirstIp()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        httpContext.Request.Headers[XForwardedForHeader] = "192.168.1.1, 10.0.0.1, 172.16.0.1";

        // Act
        var result = HttpContextHelper.GetClientIpAddress(httpContext);

        // Assert
        Assert.Equal(TestIpAddress, result);
    }

    [Fact]
    public void GetClientIpAddress_WithXForwardedForAndSpaces_ShouldReturnFirstIp()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        httpContext.Request.Headers[XForwardedForHeader] = "  192.168.1.1  , 10.0.0.1, 172.16.0.1";

        // Act
        var result = HttpContextHelper.GetClientIpAddress(httpContext);

        // Assert
        Assert.Equal(TestIpAddress, result);
    }

    [Fact]
    public void GetClientIpAddress_WithXForwardedForEmptyFirst_ShouldReturnSecondIp()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        httpContext.Request.Headers[XForwardedForHeader] = ", 192.168.1.1, 10.0.0.1";

        // Act
        var result = HttpContextHelper.GetClientIpAddress(httpContext);

        // Assert
        Assert.Equal(TestIpAddress, result);
    }

    [Fact]
    public void GetClientIpAddress_WithXRealIp_ShouldReturnRealIp()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        httpContext.Request.Headers[XRealIpHeader] = TestIpAddress2;

        // Act
        var result = HttpContextHelper.GetClientIpAddress(httpContext);

        // Assert
        Assert.Equal(TestIpAddress2, result);
    }

    [Fact]
    public void GetClientIpAddress_WithXRealIpAndXForwardedFor_ShouldReturnXForwardedFor()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        httpContext.Request.Headers[XForwardedForHeader] = TestIpAddress;
        httpContext.Request.Headers[XRealIpHeader] = TestIpAddress2;

        // Act
        var result = HttpContextHelper.GetClientIpAddress(httpContext);

        // Assert
        Assert.Equal(TestIpAddress, result);
    }

    [Fact]
    public void GetClientIpAddress_WithNoHeaders_ShouldReturnRemoteIp()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse(TestIpAddress);

        // Act
        var result = HttpContextHelper.GetClientIpAddress(httpContext);

        // Assert
        Assert.Equal(TestIpAddress, result);
    }

    [Fact]
    public void GetClientIpAddress_WithNoHeadersAndNoRemoteIp_ShouldReturnUnknown()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        httpContext.Connection.RemoteIpAddress = null;

        // Act
        var result = HttpContextHelper.GetClientIpAddress(httpContext);

        // Assert
        Assert.Equal("unknown", result);
    }

    [Fact]
    public void GetClientIpAddress_WithEmptyXForwardedFor_ShouldReturnXRealIp()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        httpContext.Request.Headers[XForwardedForHeader] = "";
        httpContext.Request.Headers[XRealIpHeader] = TestIpAddress2;

        // Act
        var result = HttpContextHelper.GetClientIpAddress(httpContext);

        // Assert
        Assert.Equal(TestIpAddress2, result);
    }

    [Fact]
    public void GetClientIpAddress_WithWhitespaceXForwardedFor_ShouldReturnXRealIp()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        httpContext.Request.Headers[XForwardedForHeader] = "   ";
        httpContext.Request.Headers[XRealIpHeader] = TestIpAddress2;

        // Act
        var result = HttpContextHelper.GetClientIpAddress(httpContext);

        // Assert
        Assert.Equal(TestIpAddress2, result);
    }

    [Fact]
    public void GetClientIpAddress_WithEmptyXRealIp_ShouldReturnRemoteIp()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        httpContext.Request.Headers[XRealIpHeader] = "";
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse(TestIpAddress);

        // Act
        var result = HttpContextHelper.GetClientIpAddress(httpContext);

        // Assert
        Assert.Equal(TestIpAddress, result);
    }

    [Fact]
    public void GetClientIpAddress_WithWhitespaceXRealIp_ShouldReturnRemoteIp()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        httpContext.Request.Headers[XRealIpHeader] = "   ";
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse(TestIpAddress);

        // Act
        var result = HttpContextHelper.GetClientIpAddress(httpContext);

        // Assert
        Assert.Equal(TestIpAddress, result);
    }

    #endregion

    #region Helper Methods

    private static DefaultHttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
        return httpContext;
    }

    #endregion
}
