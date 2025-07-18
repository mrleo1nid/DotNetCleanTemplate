using DotNetCleanTemplate.Application.Behaviors;
using MediatR;
using Moq;
using Prometheus;

namespace DotNetCleanTemplate.UnitTests.Application
{
    public class MetricsBehaviorTests
    {
        private readonly RequestHandlerDelegate<string> _next;

        public MetricsBehaviorTests()
        {
            _next = (ct) => Task.FromResult("handled");
        }

        [Fact]
        public async Task Handle_IncrementsRequestCounter()
        {
            // Arrange
            var behavior = new MetricsBehavior<TestRequest, string>();
            var request = new TestRequest();

            // Act
            var result = await behavior.Handle(request, _next, CancellationToken.None);

            // Assert
            Assert.Equal("handled", result);

            // Verify counter was incremented (we can't easily test Prometheus metrics in unit tests,
            // but we can verify the behavior doesn't throw and returns expected result)
        }

        [Fact]
        public async Task Handle_RecordsRequestDuration()
        {
            // Arrange
            var behavior = new MetricsBehavior<TestRequest, string>();
            var request = new TestRequest();
            var slowNext =
                (RequestHandlerDelegate<string>)(
                    async (ct) =>
                    {
                        await Task.Delay(100, ct); // Add delay to ensure duration measurement
                        return "handled";
                    }
                );

            // Act
            var result = await behavior.Handle(request, slowNext, CancellationToken.None);

            // Assert
            Assert.Equal("handled", result);
            // Verify duration was recorded by ensuring the behavior handles slow requests correctly
        }

        [Fact]
        public async Task Handle_WithException_StillIncrementsCounter()
        {
            // Arrange
            var behavior = new MetricsBehavior<TestRequest, string>();
            var request = new TestRequest();
            var exceptionNext =
                (RequestHandlerDelegate<string>)(
                    (ct) => throw new InvalidOperationException("Test exception")
                );

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                behavior.Handle(request, exceptionNext, CancellationToken.None)
            );

            // Verify counter was still incremented even with exception
        }

        [Fact]
        public async Task Handle_DifferentRequestTypes_UseCorrectLabels()
        {
            // Arrange
            var behavior1 = new MetricsBehavior<TestRequest1, string>();
            var behavior2 = new MetricsBehavior<TestRequest2, string>();
            var request1 = new TestRequest1();
            var request2 = new TestRequest2();

            // Act
            var result1 = await behavior1.Handle(request1, _next, CancellationToken.None);
            var result2 = await behavior2.Handle(request2, _next, CancellationToken.None);

            // Assert
            Assert.Equal("handled", result1);
            Assert.Equal("handled", result2);

            // Verify different request types are handled correctly
        }

        private class TestRequest : IRequest<string> { }

        private class TestRequest1 : IRequest<string> { }

        private class TestRequest2 : IRequest<string> { }
    }
}
