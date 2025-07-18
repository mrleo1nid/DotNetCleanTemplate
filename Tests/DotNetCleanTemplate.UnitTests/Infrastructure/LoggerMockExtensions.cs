using Microsoft.Extensions.Logging;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Infrastructure
{
    // Вспомогательный extension для проверки логов
    public static class LoggerMockExtensions
    {
        public static void VerifyLog<T>(
            this Mock<ILogger<T>> logger,
            System.Linq.Expressions.Expression<System.Action<ILogger<T>>> expression,
            Times times
        )
        {
            logger.Verify(expression, times);
        }
    }
}
