using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.UnitTests.Common;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Application
{
    public class ApplicationMigrationServiceTests : ServiceTestBase
    {
        [Fact]
        public async Task Test_ApplicationMigrationService_ApplyMigrationsIfEnabledAsync_CallsUnderlyingService()
        {
            // Arrange
            var mockMigrationService = new Mock<IMigrationService>();
            var service = new ApplicationMigrationService(mockMigrationService.Object);

            // Act
            await service.ApplyMigrationsIfEnabledAsync();

            // Assert
            mockMigrationService.Verify(
                x => x.ApplyMigrationsIfEnabledAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Test_ApplicationMigrationService_ApplyMigrationsIfEnabledAsync_HandlesExceptions()
        {
            // Arrange
            var mockMigrationService = new Mock<IMigrationService>();
            mockMigrationService
                .Setup(x => x.ApplyMigrationsIfEnabledAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Migration failed"));

            var service = new ApplicationMigrationService(mockMigrationService.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ApplyMigrationsIfEnabledAsync()
            );
            Assert.Equal("Migration failed", exception.Message);
        }

        [Fact]
        public async Task Test_ApplicationMigrationService_ApplyMigrationsIfEnabledAsync_WithCancellationToken()
        {
            // Arrange
            var mockMigrationService = new Mock<IMigrationService>();
            var service = new ApplicationMigrationService(mockMigrationService.Object);
            var cancellationToken = new CancellationToken();

            // Act
            await service.ApplyMigrationsIfEnabledAsync(cancellationToken);

            // Assert
            mockMigrationService.Verify(
                x => x.ApplyMigrationsIfEnabledAsync(cancellationToken),
                Times.Once
            );
        }

        [Fact]
        public async Task Test_ApplicationMigrationService_ApplyMigrationsIfEnabledAsync_WithCancelledToken()
        {
            // Arrange
            var mockMigrationService = new Mock<IMigrationService>();
            mockMigrationService
                .Setup(x => x.ApplyMigrationsIfEnabledAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var service = new ApplicationMigrationService(mockMigrationService.Object);
            using var cancellationTokenSource = new CancellationTokenSource();
            await cancellationTokenSource.CancelAsync();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                service.ApplyMigrationsIfEnabledAsync(cancellationTokenSource.Token)
            );
        }
    }
}
