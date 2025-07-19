using DotNetCleanTemplate.Api.DependencyExtensions;
using DotNetCleanTemplate.Api.Helpers;
using DotNetCleanTemplate.Application.DependencyExtensions;
using DotNetCleanTemplate.Infrastructure.Configurations;
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
using System.Threading.RateLimiting;

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
            if (_isTestEnvironment)
                return this;

            _builder
                .Configuration.AddJsonFile(
                    "configs/appsettings.json",
                    optional: false,
                    reloadOnChange: true
                )
                .AddJsonFile("configs/cache.json", optional: false, reloadOnChange: true)
                .AddJsonFile("configs/initData.json", optional: true, reloadOnChange: true)
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
            AddJwtAuth();
            AddDatabase();
            AddRateLimiting();
            _builder.Services.AddInfrastructure(_builder.Configuration);
            _builder.Services.AddApplicationServices(_builder.Configuration);
            _builder.Services.AddCors(_builder.Configuration);
            _builder.Services.AddLogging(_builder.Configuration, _builder.Environment);

            _builder.Services.AddFastEndpoints().SwaggerDocument();
            return this;
        }

        public void AddJwtAuth()
        {
            if (
                _builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                == null
            )
                throw new InvalidOperationException($"{JwtSettings.SectionName} is not set.");

            // JWT Auth
            var jwtSection = _builder.Configuration.GetSection(JwtSettings.SectionName);
            var jwtSettings = jwtSection.Get<JwtSettings>();
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
            if (_builder.Configuration.GetConnectionString("DefaultConnection") == null)
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' is not set."
                );

            _builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_builder.Configuration.GetConnectionString("DefaultConnection"))
            );
        }

        public void AddRateLimiting()
        {
            if (
                _builder
                    .Configuration.GetSection(RateLimitingSettings.SectionName)
                    .Get<RateLimitingSettings>() == null
            )
                throw new InvalidOperationException(
                    $"{RateLimitingSettings.SectionName} is not set."
                );
            var rateLimitingSettings =
                _builder
                    .Configuration.GetSection(RateLimitingSettings.SectionName)
                    .Get<RateLimitingSettings>() ?? new RateLimitingSettings();

            _builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Обработчик отклоненных запросов
                options.OnRejected = async (context, cancellationToken) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter = (
                            (int)retryAfter.TotalSeconds
                        ).ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                    }

                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsync(
                        "Too many requests. Please try again later.",
                        cancellationToken
                    );
                };

                // Создаем цепные ограничители
                var limiters = new List<PartitionedRateLimiter<HttpContext>>();

                // Ограничитель по IP адресу
                if (rateLimitingSettings.UseIpPartition)
                {
                    limiters.Add(
                        PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                        {
                            var ipAddress = HttpContextHelper.GetClientIpAddress(httpContext);
                            return RateLimitPartition.GetFixedWindowLimiter(
                                $"ip:{ipAddress}",
                                _ => new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = rateLimitingSettings.IpPermitLimit,
                                    Window = TimeSpan.FromSeconds(
                                        rateLimitingSettings.IpWindowSeconds
                                    ),
                                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                    QueueLimit = rateLimitingSettings.QueueLimit,
                                    AutoReplenishment = true,
                                }
                            );
                        })
                    );
                }

                // Ограничитель по API ключу
                if (rateLimitingSettings.UseApiKeyPartition)
                {
                    limiters.Add(
                        PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                        {
                            var apiKey = httpContext
                                .Request.Headers[rateLimitingSettings.ApiKeyHeaderName]
                                .FirstOrDefault();
                            var partitionKey = !string.IsNullOrEmpty(apiKey)
                                ? $"api_key:{apiKey}"
                                : "no_api_key";

                            return RateLimitPartition.GetFixedWindowLimiter(
                                partitionKey,
                                _ => new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = rateLimitingSettings.ApiKeyPermitLimit,
                                    Window = TimeSpan.FromSeconds(
                                        rateLimitingSettings.ApiKeyWindowSeconds
                                    ),
                                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                    QueueLimit = rateLimitingSettings.QueueLimit,
                                    AutoReplenishment = true,
                                }
                            );
                        })
                    );
                }

                // Если ни один из ограничителей не включен, создаем глобальный
                if (limiters.Count == 0)
                {
                    limiters.Add(
                        PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                        {
                            return RateLimitPartition.GetFixedWindowLimiter(
                                "global",
                                _ => new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = rateLimitingSettings.PermitLimit,
                                    Window = TimeSpan.FromSeconds(
                                        rateLimitingSettings.WindowSeconds
                                    ),
                                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                    QueueLimit = rateLimitingSettings.QueueLimit,
                                    AutoReplenishment = true,
                                }
                            );
                        })
                    );
                }

                // Устанавливаем глобальный ограничитель как цепочку
                options.GlobalLimiter = PartitionedRateLimiter.CreateChained(limiters.ToArray());
            });
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
