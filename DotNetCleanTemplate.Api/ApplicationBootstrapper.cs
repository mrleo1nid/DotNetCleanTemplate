using DotNetCleanTemplate.Api.DependencyExtensions;
using DotNetCleanTemplate.Application.DependencyExtensions;
using DotNetCleanTemplate.Infrastructure.DependencyExtensions;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetEnv;
using DotNetEnv.Configuration;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetEnvExtensions;
using System.Text;

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
                    "configs/appsettings.json",
                    optional: false,
                    reloadOnChange: true
                )
                .AddJsonFile("configs/cache.json", optional: false, reloadOnChange: true)
#if DEBUG
                .AddDotNetEnv(".env", LoadOptions.TraversePath())
#endif
                .AddEnvironmentVariableSubstitution();

            AddJwtAuth();
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

        public void AddJwtAuth()
        {
            // JWT Auth
            var jwtSection = _builder.Configuration.GetSection("JwtSettings");
            var jwtSettings =
                jwtSection.Get<DotNetCleanTemplate.Infrastructure.Configurations.JwtSettings>();
            _builder
                .Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings!.Issuer,
                        ValidAudience = jwtSettings!.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings!.Key)
                        ),
                        ClockSkew = TimeSpan.Zero,
                    };
                });
            _builder.Services.AddAuthorization();
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
