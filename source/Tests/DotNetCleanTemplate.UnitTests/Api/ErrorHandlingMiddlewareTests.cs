using System.Net;
using System.Text.Json;
using DotNetCleanTemplate.Api.Handlers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Api
{
    public class ErrorHandlingMiddlewareTestsFixed
    {
        [Fact]
        public async Task Test_ErrorHandlingMiddleware_WithDevelopmentEnvironment()
        {
            // Arrange
            var logger = new Mock<ILogger<ErrorHandlingMiddleware>>();
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.EnvironmentName).Returns("Development");

            var middleware = new ErrorHandlingMiddleware(
                (context) => throw new InvalidOperationException("Test exception"),
                logger.Object,
                env.Object
            );

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(httpContext);

            // Assert
            // Middleware перехватывает исключения, поэтому исключение не должно "всплывать"
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(
                responseBody
            );

            Assert.Equal((int)HttpStatusCode.InternalServerError, httpContext.Response.StatusCode);
            Assert.Equal("application/json", httpContext.Response.ContentType);
            Assert.Equal("An unexpected error occurred.", errorResponse!["error"]);
            Assert.Equal("Test exception", errorResponse!["details"]);
        }

        [Fact]
        public async Task Test_ErrorHandlingMiddleware_WithProductionEnvironment()
        {
            // Arrange
            var logger = new Mock<ILogger<ErrorHandlingMiddleware>>();
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.EnvironmentName).Returns("Production");

            var middleware = new ErrorHandlingMiddleware(
                (context) => throw new InvalidOperationException("Test exception"),
                logger.Object,
                env.Object
            );

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(httpContext);

            // Assert
            // Middleware перехватывает исключения, поэтому исключение не должно "всплывать"
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(
                responseBody
            );

            Assert.Equal((int)HttpStatusCode.InternalServerError, httpContext.Response.StatusCode);
            Assert.Equal("application/json", httpContext.Response.ContentType);
            Assert.Equal("An unexpected error occurred.", errorResponse!["error"]);
            Assert.False(errorResponse!.ContainsKey("details"));
        }

        [Fact]
        public async Task Test_ErrorHandlingMiddleware_WithDifferentExceptionTypes()
        {
            // Arrange
            var logger = new Mock<ILogger<ErrorHandlingMiddleware>>();
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.EnvironmentName).Returns("Development");

            var middleware = new ErrorHandlingMiddleware(
                (context) => throw new InvalidOperationException("Test parameter is null"),
                logger.Object,
                env.Object
            );

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(httpContext);

            // Assert
            // Middleware перехватывает исключения, поэтому исключение не должно "всплывать"
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(
                responseBody
            );

            Assert.Equal((int)HttpStatusCode.InternalServerError, httpContext.Response.StatusCode);
            Assert.Equal("application/json", httpContext.Response.ContentType);
            Assert.Equal("An unexpected error occurred.", errorResponse!["error"]);
            Assert.Contains("Test parameter is null", errorResponse!["details"]);
        }

        [Fact]
        public async Task Test_ErrorHandlingMiddleware_WithNoException()
        {
            // Arrange
            var logger = new Mock<ILogger<ErrorHandlingMiddleware>>();
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.EnvironmentName).Returns("Development");

            var middleware = new ErrorHandlingMiddleware(
                async (context) =>
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("Success");
                },
                logger.Object,
                env.Object
            );

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(httpContext);

            // Assert
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();

            Assert.Equal(200, httpContext.Response.StatusCode);
            Assert.Equal("Success", responseBody);
        }

        [Fact]
        public async Task Test_ErrorHandlingMiddleware_WithStagingEnvironment()
        {
            // Arrange
            var logger = new Mock<ILogger<ErrorHandlingMiddleware>>();
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.EnvironmentName).Returns("Staging");

            var middleware = new ErrorHandlingMiddleware(
                (context) => throw new InvalidOperationException("Test exception"),
                logger.Object,
                env.Object
            );

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(httpContext);

            // Assert
            // Middleware перехватывает исключения, поэтому исключение не должно "всплывать"
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(
                responseBody
            );

            Assert.Equal((int)HttpStatusCode.InternalServerError, httpContext.Response.StatusCode);
            Assert.Equal("application/json", httpContext.Response.ContentType);
            Assert.Equal("An unexpected error occurred.", errorResponse!["error"]);
            Assert.False(errorResponse!.ContainsKey("details")); // Staging не считается Development
        }

        [Fact]
        public void Test_ErrorHandlingMiddleware_Constructor_WithValidParameters()
        {
            // Arrange & Act
            var logger = new Mock<ILogger<ErrorHandlingMiddleware>>();
            var env = new Mock<IWebHostEnvironment>();
            var next = new RequestDelegate(async (context) => await Task.CompletedTask);

            var middleware = new ErrorHandlingMiddleware(next, logger.Object, env.Object);

            // Assert
            Assert.NotNull(middleware);
        }

        [Fact]
        public async Task Test_ErrorHandlingMiddleware_WithAggregateException()
        {
            // Arrange
            var logger = new Mock<ILogger<ErrorHandlingMiddleware>>();
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.EnvironmentName).Returns("Development");

            var middleware = new ErrorHandlingMiddleware(
                (context) =>
                    throw new AggregateException(
                        "Multiple exceptions",
                        new InvalidOperationException("First exception"),
                        new ArgumentException("Second exception")
                    ),
                logger.Object,
                env.Object
            );

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(httpContext);

            // Assert
            // Middleware перехватывает исключения, поэтому исключение не должно "всплывать"
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(
                responseBody
            );

            Assert.Equal((int)HttpStatusCode.InternalServerError, httpContext.Response.StatusCode);
            Assert.Equal("application/json", httpContext.Response.ContentType);
            Assert.Equal("An unexpected error occurred.", errorResponse!["error"]);
            Assert.Contains("Multiple exceptions", errorResponse!["details"]);
        }
    }
}
