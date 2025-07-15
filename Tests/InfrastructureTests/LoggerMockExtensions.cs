using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;

namespace InfrastructureTests
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
