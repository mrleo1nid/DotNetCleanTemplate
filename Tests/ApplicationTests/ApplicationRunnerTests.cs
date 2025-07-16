using System;
using System.Threading.Tasks;
using DotNetCleanTemplate.Api;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ApplicationTests
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
