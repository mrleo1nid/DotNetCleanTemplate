using System.Net;
using DotNetCleanTemplate.Api.Helpers;
using DotNetCleanTemplate.Infrastructure.Configurations;
using Microsoft.AspNetCore.Http;

namespace DotNetCleanTemplate.UnitTests.Api
{
    public class HttpContextHelperTests
    {
        [Fact]
        public void Test_GetClientIpAddress_WithXForwardedForHeader()
        {
            // Arrange
            var httpContext = CreateHttpContext();
            httpContext.Request.Headers["X-Forwarded-For"] = "192.168.1.100, 10.0.0.1";

            // Act
            var result = HttpContextHelper.GetClientIpAddress(httpContext);

            // Assert
            Assert.Equal("192.168.1.100", result);
        }

        [Fact]
        public void Test_GetClientIpAddress_WithMultipleXForwardedForHeaders()
        {
            // Arrange
            var httpContext = CreateHttpContext();
            httpContext.Request.Headers["X-Forwarded-For"] =
                "203.0.113.1, 198.51.100.1, 192.168.1.1";

            // Act
            var result = HttpContextHelper.GetClientIpAddress(httpContext);

            // Assert
            Assert.Equal("203.0.113.1", result);
        }

        [Fact]
        public void Test_GetClientIpAddress_WithInvalidIpAddress()
        {
            // Arrange
            var httpContext = CreateHttpContext();
            httpContext.Request.Headers["X-Forwarded-For"] = "invalid-ip-address";

            // Act
            var result = HttpContextHelper.GetClientIpAddress(httpContext);

            // Assert
            Assert.Equal("invalid-ip-address", result);
        }

        [Fact]
        public void Test_GetClientIpAddress_WithPrivateIpAddress()
        {
            // Arrange
            var httpContext = CreateHttpContext();
            httpContext.Request.Headers["X-Forwarded-For"] = "10.0.0.1";

            // Act
            var result = HttpContextHelper.GetClientIpAddress(httpContext);

            // Assert
            Assert.Equal("10.0.0.1", result);
        }

        [Fact]
        public void Test_GetClientIpAddress_WithXRealIpHeader()
        {
            // Arrange
            var httpContext = CreateHttpContext();
            httpContext.Request.Headers["X-Real-IP"] = "203.0.113.1";

            // Act
            var result = HttpContextHelper.GetClientIpAddress(httpContext);

            // Assert
            Assert.Equal("203.0.113.1", result);
        }

        [Fact]
        public void Test_GetClientIpAddress_WithRemoteIpAddress()
        {
            // Arrange
            var httpContext = CreateHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");

            // Act
            var result = HttpContextHelper.GetClientIpAddress(httpContext);

            // Assert
            Assert.Equal("192.168.1.100", result);
        }

        [Fact]
        public void Test_GetClientIpAddress_WithNullRemoteIpAddress()
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
        public void Test_GetPartitionKey_WithApiKeyPartition()
        {
            // Arrange
            var httpContext = CreateHttpContext();
            httpContext.Request.Headers["X-API-Key"] = "test-api-key";
            var settings = new RateLimitingSettings
            {
                UseApiKeyPartition = true,
                UseIpPartition = false,
                ApiKeyHeaderName = "X-API-Key",
            };

            // Act
            var result = HttpContextHelper.GetPartitionKey(httpContext, settings);

            // Assert
            Assert.Equal("api_key:test-api-key", result);
        }

        [Fact]
        public void Test_GetPartitionKey_WithIpPartition()
        {
            // Arrange
            var httpContext = CreateHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");
            var settings = new RateLimitingSettings
            {
                UseApiKeyPartition = false,
                UseIpPartition = true,
            };

            // Act
            var result = HttpContextHelper.GetPartitionKey(httpContext, settings);

            // Assert
            Assert.Equal("ip:192.168.1.100", result);
        }

        [Fact]
        public void Test_GetPartitionKey_WithBothPartitions_ApiKeyFirst()
        {
            // Arrange
            var httpContext = CreateHttpContext();
            httpContext.Request.Headers["X-API-Key"] = "test-api-key";
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");
            var settings = new RateLimitingSettings
            {
                UseApiKeyPartition = true,
                UseIpPartition = true,
                ApiKeyHeaderName = "X-API-Key",
            };

            // Act
            var result = HttpContextHelper.GetPartitionKey(httpContext, settings);

            // Assert
            Assert.Equal("api_key:test-api-key", result);
        }

        [Fact]
        public void Test_GetPartitionKey_WithBothPartitions_IpFallback()
        {
            // Arrange
            var httpContext = CreateHttpContext();
            // Не добавляем API ключ
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");
            var settings = new RateLimitingSettings
            {
                UseApiKeyPartition = true,
                UseIpPartition = true,
                ApiKeyHeaderName = "X-API-Key",
            };

            // Act
            var result = HttpContextHelper.GetPartitionKey(httpContext, settings);

            // Assert
            Assert.Equal("ip:192.168.1.100", result);
        }

        [Fact]
        public void Test_GetPartitionKey_WithGlobalFallback()
        {
            // Arrange
            var httpContext = CreateHttpContext();
            var settings = new RateLimitingSettings
            {
                UseApiKeyPartition = false,
                UseIpPartition = false,
            };

            // Act
            var result = HttpContextHelper.GetPartitionKey(httpContext, settings);

            // Assert
            Assert.Equal("global", result);
        }

        [Fact]
        public void Test_GetPartitionKey_WithEmptyApiKey()
        {
            // Arrange
            var httpContext = CreateHttpContext();
            httpContext.Request.Headers["X-API-Key"] = "";
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");
            var settings = new RateLimitingSettings
            {
                UseApiKeyPartition = true,
                UseIpPartition = true,
                ApiKeyHeaderName = "X-API-Key",
            };

            // Act
            var result = HttpContextHelper.GetPartitionKey(httpContext, settings);

            // Assert
            Assert.Equal("ip:192.168.1.100", result);
        }

        private static HttpContext CreateHttpContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            return httpContext;
        }
    }
}
