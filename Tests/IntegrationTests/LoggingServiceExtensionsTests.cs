using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Sdk;

namespace IntegrationTests
{
    public class LoggingServiceExtensionsTests : TestBase
    {
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
