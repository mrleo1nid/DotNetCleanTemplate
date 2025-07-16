using System.Net;
using DotNetCleanTemplate.Api;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class LoggingServiceExtensionsTests : TestBase
    {
        public LoggingServiceExtensionsTests(
            CustomWebApplicationFactory<Program> factory,
            ITestOutputHelper output
        )
            : base(factory, output) { }

        [Fact]
        public void Logging_Services_Are_Registered_In_DI()
        {
            using var scope = Factory
                .Services.GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            var provider = scope.ServiceProvider;

            provider.GetService<ILoggerFactory>().Should().NotBeNull();
            provider.GetService<Serilog.ILogger>().Should().NotBeNull();
            provider.GetService<ILogger<LoggingServiceExtensionsTests>>().Should().NotBeNull();
        }
    }
}
