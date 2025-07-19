using DotNetCleanTemplate.Application.Interfaces;
using System.Security.Claims;

namespace DotNetCleanTemplate.Api.Handlers
{
    public class UserLockoutMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserLockoutMiddleware> _logger;

        public UserLockoutMiddleware(RequestDelegate next, ILogger<UserLockoutMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IUserLockoutService userLockoutService)
        {
            // Проверяем, аутентифицирован ли пользователь
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (
                    !string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId)
                )
                {
                    // Проверяем блокировку пользователя
                    var lockoutCheck = await userLockoutService.CheckUserLockoutAsync(
                        userId,
                        context.RequestAborted
                    );

                    if (!lockoutCheck.IsSuccess)
                    {
                        _logger.LogWarning(
                            "Blocked user {UserId} attempted to access protected endpoint",
                            userId
                        );

                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var errorResponse = new
                        {
                            error = "Account is temporarily locked due to multiple failed login attempts. Please try again later.",
                            code = "Auth.UserLocked",
                        };

                        await context.Response.WriteAsJsonAsync(
                            errorResponse,
                            context.RequestAborted
                        );
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
