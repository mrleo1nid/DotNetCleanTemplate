using Microsoft.Extensions.Logging;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Common
{
    public static class MoqExtensions
    {
        public static void VerifyLog<T>(
            this Mock<ILogger<T>> logger,
            System.Linq.Expressions.Expression<System.Action<ILogger<T>>> expression,
            Times times
        )
        {
            logger.Verify(expression, times);
        }

        public static void VerifyLog<T>(
            this Mock<ILogger<T>> logger,
            System.Linq.Expressions.Expression<System.Action<ILogger<T>>> expression
        )
        {
            logger.Verify(expression, Times.Once);
        }

        public static Mock<T> CreateMock<T>()
            where T : class
        {
            return new Mock<T>();
        }

        public static Mock<T> CreateStrictMock<T>()
            where T : class
        {
            return new Mock<T>(MockBehavior.Strict);
        }

        public static Mock<T> CreateLooseMock<T>()
            where T : class
        {
            return new Mock<T>(MockBehavior.Loose);
        }
    }
}
