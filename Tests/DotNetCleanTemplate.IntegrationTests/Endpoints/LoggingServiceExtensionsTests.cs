using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetCleanTemplate.IntegrationTests.Endpoints
{
    public class LoggingServiceExtensionsTests : TestBase
    {
        public LoggingServiceExtensionsTests(CustomWebApplicationFactory<Program> factory)
            : base(factory) { }

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
