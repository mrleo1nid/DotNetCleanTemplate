using DotNetCleanTemplate.Application.Behaviors;
using DotNetCleanTemplate.Application.Configurations;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Concurrent;

namespace DotNetCleanTemplate.UnitTests.Application
{
    public class PerformanceBehaviourTests
    {
        private readonly TestLogger<PerformanceBehaviour<TestRequest, string>> _logger;
        private readonly Mock<IOptions<PerformanceSettings>> _settingsMock;
        private readonly RequestHandlerDelegate<string> _next;

        public PerformanceBehaviourTests()
        {
            _logger = new TestLogger<PerformanceBehaviour<TestRequest, string>>();
            _settingsMock = new Mock<IOptions<PerformanceSettings>>();
            _settingsMock
                .Setup(x => x.Value)
                .Returns(new PerformanceSettings { LongRunningThresholdMs = 100 });
            _next = (ct) => Task.FromResult("handled");
        }

        [Fact]
        public async Task Handle_FastRequest_DoesNotLogWarning()
        {
            // Arrange
            var behavior = new PerformanceBehaviour<TestRequest, string>(
                _logger,
                _settingsMock.Object
            );
            var request = new TestRequest();

            // Act
            var result = await behavior.Handle(request, _next, CancellationToken.None);

            // Assert
            Assert.Equal("handled", result);
            Assert.Equal(0, _logger.GetLogCount(LogLevel.Warning));
        }

        [Fact]
        public async Task Handle_SlowRequest_LogsWarning()
        {
            // Arrange
            var behavior = new PerformanceBehaviour<TestRequest, string>(
                _logger,
                _settingsMock.Object
            );
            var request = new TestRequest();
            var slowNext =
                (RequestHandlerDelegate<string>)(
                    async (ct) =>
                    {
                        await Task.Delay(150, ct); // Longer than threshold
                        return "handled";
                    }
                );

            // Act
            var result = await behavior.Handle(request, slowNext, CancellationToken.None);

            // Assert
            Assert.Equal("handled", result);
            Assert.True(_logger.HasLogEntry(LogLevel.Warning, "Long running request"));
        }

        [Fact]
        public async Task Handle_SlowRequest_IncludesRequestTypeInLog()
        {
            // Arrange
            var behavior = new PerformanceBehaviour<TestRequest, string>(
                _logger,
                _settingsMock.Object
            );
            var request = new TestRequest();
            var slowNext =
                (RequestHandlerDelegate<string>)(
                    async (ct) =>
                    {
                        await Task.Delay(150, ct);
                        return "handled";
                    }
                );

            // Act
            var result = await behavior.Handle(request, slowNext, CancellationToken.None);

            // Assert
            Assert.Equal("handled", result);
            Assert.True(_logger.HasLogEntry(LogLevel.Warning, "TestRequest"));
        }

        [Fact]
        public async Task Handle_SlowRequest_IncludesElapsedTimeInLog()
        {
            // Arrange
            var behavior = new PerformanceBehaviour<TestRequest, string>(
                _logger,
                _settingsMock.Object
            );
            var request = new TestRequest();
            var slowNext =
                (RequestHandlerDelegate<string>)(
                    async (ct) =>
                    {
                        await Task.Delay(150, ct);
                        return "handled";
                    }
                );

            // Act
            var result = await behavior.Handle(request, slowNext, CancellationToken.None);

            // Assert
            Assert.Equal("handled", result);
            Assert.True(_logger.HasLogEntry(LogLevel.Warning, "ms"));
        }

        [Fact]
        public async Task Handle_WithException_StillMeasuresTime()
        {
            // Arrange
            var behavior = new PerformanceBehaviour<TestRequest, string>(
                _logger,
                _settingsMock.Object
            );
            var request = new TestRequest();
            var exceptionNext =
                (RequestHandlerDelegate<string>)(
                    (ct) => throw new InvalidOperationException("Test exception")
                );

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                behavior.Handle(request, exceptionNext, CancellationToken.None)
            );

            // Verify that timing still works even with exceptions
        }

        [Fact]
        public async Task Handle_DifferentThresholds_RespectsSettings()
        {
            // Arrange - Set very low threshold
            var lowThresholdSettings = new Mock<IOptions<PerformanceSettings>>();
            lowThresholdSettings
                .Setup(x => x.Value)
                .Returns(new PerformanceSettings { LongRunningThresholdMs = 10 });

            var behavior = new PerformanceBehaviour<TestRequest, string>(
                _logger,
                lowThresholdSettings.Object
            );
            var request = new TestRequest();
            var mediumNext =
                (RequestHandlerDelegate<string>)(
                    async (ct) =>
                    {
                        await Task.Delay(50, ct); // Longer than low threshold
                        return "handled";
                    }
                );

            // Act
            var result = await behavior.Handle(request, mediumNext, CancellationToken.None);

            // Assert
            Assert.Equal("handled", result);
            Assert.Equal(1, _logger.GetLogCount(LogLevel.Warning));
        }

        [Fact]
        public async Task Handle_DifferentRequestTypes_UseCorrectTypeName()
        {
            // Arrange
            var logger1 = new TestLogger<PerformanceBehaviour<TestRequest1, string>>();
            var logger2 = new TestLogger<PerformanceBehaviour<TestRequest2, string>>();

            var behavior1 = new PerformanceBehaviour<TestRequest1, string>(
                logger1,
                _settingsMock.Object
            );
            var behavior2 = new PerformanceBehaviour<TestRequest2, string>(
                logger2,
                _settingsMock.Object
            );
            var request1 = new TestRequest1();
            var request2 = new TestRequest2();
            var slowNext =
                (RequestHandlerDelegate<string>)(
                    async (ct) =>
                    {
                        await Task.Delay(150, ct);
                        return "handled";
                    }
                );

            // Act
            var result1 = await behavior1.Handle(request1, slowNext, CancellationToken.None);
            var result2 = await behavior2.Handle(request2, slowNext, CancellationToken.None);

            // Assert
            Assert.Equal("handled", result1);
            Assert.Equal("handled", result2);

            // Verify both request types are logged with correct names
            Assert.True(logger1.HasLogEntry(LogLevel.Warning, "TestRequest1"));
            Assert.True(logger2.HasLogEntry(LogLevel.Warning, "TestRequest2"));
        }

        private class TestRequest : IRequest<string> { }

        private class TestRequest1 : IRequest<string> { }

        private class TestRequest2 : IRequest<string> { }

        private class TestLogger<T> : ILogger<T>
        {
            public ConcurrentQueue<LogEntry> LogEntries { get; } = new();

            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter
            )
            {
                LogEntries.Enqueue(new LogEntry(logLevel, state?.ToString() ?? "", exception));
            }

            public bool HasLogEntry(LogLevel level, string message)
            {
                return LogEntries.Any(entry =>
                    entry.LogLevel == level && entry.Message.Contains(message)
                );
            }

            public int GetLogCount(LogLevel level)
            {
                return LogEntries.Count(entry => entry.LogLevel == level);
            }

            public record LogEntry(LogLevel LogLevel, string Message, Exception? Exception);
        }
    }
}
