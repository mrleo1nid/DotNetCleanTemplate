using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.Infrastructure.Configurations;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Threading.RateLimiting;

namespace DotNetCleanTemplate.UnitTests.Api
{
    public class ApplicationRunnerTests
    {
        private static WebApplication CreateAppMock()
        {
            Environment.SetEnvironmentVariable("IsTestEnvironment", "Test");
            var builder = WebApplication.CreateBuilder();

            // Добавляем необходимые сервисы для middleware
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();
            builder.Services.AddFastEndpoints();
            builder.Services.AddSingleton<IMediator>(Mock.Of<IMediator>());
            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter(
                    RateLimitingSettings.PolicyName,
                    opt =>
                    {
                        opt.PermitLimit = 100;
                        opt.Window = TimeSpan.FromSeconds(60);
                        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                        opt.QueueLimit = 10;
                    }
                );
            });
            return builder.Build();
        }

        [Fact]
        public void ConfigureMiddleware_DoesNotThrow()
        {
            Environment.SetEnvironmentVariable("IsTestEnvironment", "Test");
            var app = CreateAppMock();
            var runner = new ApplicationRunner(app);
            var ex = Record.Exception(() => runner.ConfigureMiddleware());
            Assert.Null(ex);
        }

        [Fact]
        public void MapEndpoints_DoesNotThrow()
        {
            Environment.SetEnvironmentVariable("IsTestEnvironment", "Test");
            var app = CreateAppMock();
            var runner = new ApplicationRunner(app);
            var ex = Record.Exception(() => runner.MapEndpoints());
            Assert.Null(ex);
        }

        [Fact]
        public async Task RunAsync_Throws_WhenExceptionOccurs()
        {
            Environment.SetEnvironmentVariable("IsTestEnvironment", "Test");
            var app = CreateAppMock();
            var runner = new ApplicationRunner(app);
            // Симулируем ошибку, например, через отсутствие миграционного сервиса
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await runner.RunAsync()
            );
        }
    }
}
