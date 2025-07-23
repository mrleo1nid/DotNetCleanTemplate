using DotNetCleanTemplate.Api.DependencyExtensions;
using DotNetCleanTemplate.Application.DependencyExtensions;
using DotNetCleanTemplate.Infrastructure.DependencyExtensions;
using FastEndpoints;
using FastEndpoints.Swagger;

namespace DotNetCleanTemplate.Api
{
    public class ApplicationBootstrapper
    {
        private readonly WebApplicationBuilder _builder;
        private readonly bool _isTestEnvironment;

        public ApplicationBootstrapper(WebApplicationBuilder builder)
        {
            _builder = builder;
            _isTestEnvironment =
                _builder.Configuration.GetValue<string>("IsTestEnvironment") == "Test";
        }

        /// <summary>
        /// Инициализировать конфигурацию
        /// </summary>
        public ApplicationBootstrapper InitializeConfiguration()
        {
            _builder.InitializeConfiguration(_isTestEnvironment);
            return this;
        }

        /// <summary>
        /// Настроить сервисы
        /// </summary>
        public ApplicationBootstrapper ConfigureServices()
        {
            _builder.Services.AddLogging(_builder.Configuration, _builder.Environment);
            _builder.Services.AddJwtAuthentication(_builder.Configuration);
            _builder.Services.AddDatabase(_builder.Configuration);
            _builder.Services.AddRateLimiting(_builder.Configuration);
            _builder.Services.AddInfrastructure(_builder.Configuration);
            _builder.Services.AddApplicationServices(_builder.Configuration);
            _builder.Services.AddCors(_builder.Configuration);

            _builder.Services.AddFastEndpoints().SwaggerDocument();
            return this;
        }

        /// <summary>
        /// Создать веб-приложение
        /// </summary>
        public WebApplication Build()
        {
            return _builder.Build();
        }
    }
}
