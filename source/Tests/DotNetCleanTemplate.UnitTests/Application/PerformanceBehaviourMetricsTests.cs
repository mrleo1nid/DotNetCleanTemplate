using System.Collections.Concurrent;
using System.Reflection;
using DotNetCleanTemplate.Application.Behaviors;
using DotNetCleanTemplate.Application.Configurations;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Prometheus;

namespace DotNetCleanTemplate.UnitTests.Application;

public class PerformanceBehaviourMetricsTests
{
    private const string LongRunningRequestsFieldName = "LongRunningRequests";
    private const string HandledResult = "handled";
    private const string TestRequestTypeName = "TestRequest";
    private const string TestRequest1TypeName = "TestRequest1";
    private const string TestRequest2TypeName = "TestRequest2";

    private readonly TestLogger<PerformanceBehaviour<TestRequest, string>> _logger;
    private readonly Mock<IOptions<PerformanceSettings>> _settingsMock;

    public PerformanceBehaviourMetricsTests()
    {
        _logger = new TestLogger<PerformanceBehaviour<TestRequest, string>>();
        _settingsMock = new Mock<IOptions<PerformanceSettings>>();
        _settingsMock
            .Setup(x => x.Value)
            .Returns(new PerformanceSettings { LongRunningThresholdMs = 100 });
    }
#pragma warning disable S3011
    [Fact]
    public async Task Handle_SlowRequest_ShouldIncrementCounter()
    {
        // Arrange
        var behavior = new PerformanceBehaviour<TestRequest, string>(_logger, _settingsMock.Object);
        var request = new TestRequest();
        var slowNext =
            (RequestHandlerDelegate<string>)(
                async (ct) =>
                {
                    await Task.Delay(150, ct); // Longer than threshold
                    return HandledResult;
                }
            );

        // Act
        var result = await behavior.Handle(request, slowNext, CancellationToken.None);

        // Assert
        Assert.Equal(HandledResult, result);

        var counterField = typeof(PerformanceBehaviour<TestRequest, string>).GetField(
            LongRunningRequestsFieldName,
            BindingFlags.NonPublic | BindingFlags.Static
        );
        Assert.NotNull(counterField);

        var counter = counterField.GetValue(null) as Counter;
        Assert.NotNull(counter);

        // Получаем значение метрики для типа запроса
        var metricValue = counter.WithLabels(TestRequestTypeName).Value;
        Assert.True(metricValue > 0, "Counter should be incremented for slow request");
    }

    [Fact]
    public async Task Handle_FastRequest_ShouldNotIncrementCounter()
    {
        // Arrange
        var fastLogger = new TestLogger<PerformanceBehaviour<FastTestRequest, string>>();
        var behavior = new PerformanceBehaviour<FastTestRequest, string>(
            fastLogger,
            _settingsMock.Object
        );
        var request = new FastTestRequest();
        var fastNext =
            (RequestHandlerDelegate<string>)(
                async (ct) =>
                {
                    await Task.Delay(10, ct); // Much shorter than threshold
                    return HandledResult;
                }
            );

        var counterField = typeof(PerformanceBehaviour<FastTestRequest, string>).GetField(
            LongRunningRequestsFieldName,
            BindingFlags.NonPublic | BindingFlags.Static
        );
        var counter = counterField?.GetValue(null) as Counter;
        var initialValue = counter?.WithLabels("FastTestRequest").Value ?? 0;

        // Act
        var result = await behavior.Handle(request, fastNext, CancellationToken.None);

        // Assert
        Assert.Equal(HandledResult, result);

        // Проверяем, что метрика не была увеличена для быстрого запроса
        var finalValue = counter?.WithLabels("FastTestRequest").Value ?? 0;
        // Поскольку счетчик статический, мы проверяем, что он не увеличился после выполнения запроса
        // Если счетчик был 0, он должен остаться 0
        // Если счетчик был больше 0 (из-за других тестов), он не должен увеличиться
        Assert.True(
            Math.Abs(finalValue - initialValue) < 0.001,
            $"Counter should not be incremented for fast request. Initial: {initialValue}, Final: {finalValue}"
        );
    }

    [Fact]
    public async Task Handle_DifferentRequestTypes_ShouldUseCorrectLabels()
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
                    return HandledResult;
                }
            );

        // Act
        await behavior1.Handle(request1, slowNext, CancellationToken.None);
        await behavior2.Handle(request2, slowNext, CancellationToken.None);

        var counterField1 = typeof(PerformanceBehaviour<TestRequest1, string>).GetField(
            LongRunningRequestsFieldName,
            BindingFlags.NonPublic | BindingFlags.Static
        );
        var counterField2 = typeof(PerformanceBehaviour<TestRequest2, string>).GetField(
            LongRunningRequestsFieldName,
            BindingFlags.NonPublic | BindingFlags.Static
        );

        var counter1 = counterField1?.GetValue(null) as Counter;
        var counter2 = counterField2?.GetValue(null) as Counter;

        // Проверяем, что метрики для разных типов запросов увеличились
        var value1 = counter1?.WithLabels(TestRequest1TypeName).Value ?? 0;
        var value2 = counter2?.WithLabels(TestRequest2TypeName).Value ?? 0;

        Assert.True(value1 > 0, "Counter should be incremented for TestRequest1");
        Assert.True(value2 > 0, "Counter should be incremented for TestRequest2");
    }

    [Fact]
    public async Task Handle_MultipleSlowRequests_ShouldIncrementCounterMultipleTimes()
    {
        // Arrange
        var behavior = new PerformanceBehaviour<TestRequest, string>(_logger, _settingsMock.Object);
        var request = new TestRequest();
        var slowNext =
            (RequestHandlerDelegate<string>)(
                async (ct) =>
                {
                    await Task.Delay(150, ct);
                    return HandledResult;
                }
            );

        // Act
        await behavior.Handle(request, slowNext, CancellationToken.None);
        await behavior.Handle(request, slowNext, CancellationToken.None);
        await behavior.Handle(request, slowNext, CancellationToken.None);

        var counterField = typeof(PerformanceBehaviour<TestRequest, string>).GetField(
            LongRunningRequestsFieldName,
            BindingFlags.NonPublic | BindingFlags.Static
        );
        var counter = counterField?.GetValue(null) as Counter;
        var metricValue = counter?.WithLabels(TestRequestTypeName).Value ?? 0;
        Assert.True(metricValue >= 3, "Counter should be incremented for each slow request");
    }

    [Fact]
    public async Task Handle_ExceptionInRequest_ShouldStillIncrementCounterIfSlow()
    {
        // Arrange
        var behavior = new PerformanceBehaviour<TestRequest, string>(_logger, _settingsMock.Object);
        var request = new TestRequest();
        var slowExceptionNext =
            (RequestHandlerDelegate<string>)(
                async (ct) =>
                {
                    await Task.Delay(150, ct); // Slow but will throw exception
                    throw new InvalidOperationException("Test exception");
                }
            );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(request, slowExceptionNext, CancellationToken.None)
        );

        var counterField = typeof(PerformanceBehaviour<TestRequest, string>).GetField(
            LongRunningRequestsFieldName,
            BindingFlags.NonPublic | BindingFlags.Static
        );
        var counter = counterField?.GetValue(null) as Counter;
        var metricValue = counter?.WithLabels(TestRequestTypeName).Value ?? 0;
        Assert.True(
            metricValue > 0,
            "Counter should be incremented even for slow requests that throw exceptions"
        );
    }

    [Fact]
    public async Task Handle_WithDifferentThresholds_ShouldRespectThresholdForMetrics()
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
                    return HandledResult;
                }
            );

        // Act
        var result = await behavior.Handle(request, mediumNext, CancellationToken.None);

        // Assert
        Assert.Equal(HandledResult, result);

        var counterField = typeof(PerformanceBehaviour<TestRequest, string>).GetField(
            LongRunningRequestsFieldName,
            BindingFlags.NonPublic | BindingFlags.Static
        );
        var counter = counterField?.GetValue(null) as Counter;
        var metricValue = counter?.WithLabels(TestRequestTypeName).Value ?? 0;
        Assert.True(
            metricValue > 0,
            "Counter should be incremented when request exceeds low threshold"
        );
    }

#pragma warning restore S3011

    private sealed class TestRequest : IRequest<string> { }

    private sealed class TestRequest1 : IRequest<string> { }

    private sealed class TestRequest2 : IRequest<string> { }

    private sealed class FastTestRequest : IRequest<string> { }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public ConcurrentQueue<LogEntry> LogEntries { get; } = new();

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "SonarLint",
            "S1144:Remove unused private methods",
            Justification = "Required for ILogger interface implementation"
        )]
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "SonarLint",
            "S1144:Remove unused private methods",
            Justification = "Required for ILogger interface implementation"
        )]
        public bool IsEnabled(LogLevel logLevel) => true;

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "SonarLint",
            "S1144:Remove unused private methods",
            Justification = "Required for ILogger interface implementation"
        )]
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

        public record LogEntry(LogLevel LogLevel, string Message, Exception? Exception);
    }
}
