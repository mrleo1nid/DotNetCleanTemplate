using DotNetCleanTemplate.Api.DependencyExtensions;
using DotNetCleanTemplate.Application.DependencyExtensions;
using DotNetCleanTemplate.Infrastructure.DependencyExtensions;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetEnv;
using DotNetEnv.Configuration;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using NetEnvExtensions;

namespace DotNetCleanTemplate.Api
{
    public class ApplicationBootstrapper
    {
        private readonly WebApplicationBuilder _builder;

        public ApplicationBootstrapper(WebApplicationBuilder builder)
        {
            _builder = builder;
        }

        /// <summary>
        /// Инициализировать конфигурацию
        /// </summary>
        public ApplicationBootstrapper InitializeConfiguration()
        {
            _builder
                .Configuration.AddJsonFile(
                    "/config/appsettings.json",
                    optional: false,
                    reloadOnChange: true
                )
                .AddJsonFile("/config/cache.json", optional: false, reloadOnChange: true)
#if DEBUG
                .AddDotNetEnv(".env", LoadOptions.TraversePath())
#endif
                .AddEnvironmentVariableSubstitution();

            return this;
        }

        /// <summary>
        /// Настроить сервисы
        /// </summary>
        public ApplicationBootstrapper ConfigureServices()
        {
            AddDatabase();
            _builder.Services.AddInfrastructure(_builder.Configuration);
            _builder.Services.AddApplicationServices();
            _builder.Services.AddCors(_builder.Configuration);
            _builder.Services.AddLogging(_builder.Configuration, _builder.Environment);
            _builder.Services.AddFastEndpoints().SwaggerDocument();
            return this;
        }

        public void AddDatabase()
        {
            _builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_builder.Configuration.GetConnectionString("DefaultConnection"))
            );
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
