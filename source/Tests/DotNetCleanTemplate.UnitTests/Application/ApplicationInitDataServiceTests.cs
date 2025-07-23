using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.UnitTests.Common;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Application
{
    public class ApplicationInitDataServiceTests : ServiceTestBase
    {
        [Fact]
        public async Task Test_ApplicationInitDataService_InitializeAsync_CallsUnderlyingService()
        {
            // Arrange
            var mockInitDataService = new Mock<IInitDataService>();
            var service = new ApplicationInitDataService(mockInitDataService.Object);

            // Act
            await service.InitializeAsync();

            // Assert
            mockInitDataService.Verify(
                x => x.InitializeAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Test_ApplicationInitDataService_InitializeAsync_HandlesExceptions()
        {
            // Arrange
            var mockInitDataService = new Mock<IInitDataService>();
            mockInitDataService
                .Setup(x => x.InitializeAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            var service = new ApplicationInitDataService(mockInitDataService.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.InitializeAsync()
            );
            Assert.Equal("Test exception", exception.Message);
        }

        [Fact]
        public async Task Test_ApplicationInitDataService_InitializeAsync_WithCancellationToken()
        {
            // Arrange
            var mockInitDataService = new Mock<IInitDataService>();
            var service = new ApplicationInitDataService(mockInitDataService.Object);
            var cancellationToken = new CancellationToken();

            // Act
            await service.InitializeAsync(cancellationToken);

            // Assert
            mockInitDataService.Verify(x => x.InitializeAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Test_ApplicationInitDataService_InitializeAsync_WithCancelledToken()
        {
            // Arrange
            var mockInitDataService = new Mock<IInitDataService>();
            mockInitDataService
                .Setup(x => x.InitializeAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var service = new ApplicationInitDataService(mockInitDataService.Object);
            using var cancellationTokenSource = new CancellationTokenSource();
            await cancellationTokenSource.CancelAsync();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                service.InitializeAsync(cancellationTokenSource.Token)
            );
        }
    }
}
