using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Services;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Application.Services;

public class ApplicationMigrationServiceTests
{
    private readonly Mock<IMigrationService> _migrationServiceMock;
    private readonly ApplicationMigrationService _service;

    public ApplicationMigrationServiceTests()
    {
        _migrationServiceMock = new Mock<IMigrationService>();
        _service = new ApplicationMigrationService(_migrationServiceMock.Object);
    }

    [Fact]
    public void Constructor_Should_Initialize_Dependencies()
    {
        // Assert
        Assert.NotNull(_service);
    }

    [Fact]
    public async Task ApplyMigrationsIfEnabledAsync_Should_Call_MigrationService()
    {
        // Arrange
        _migrationServiceMock
            .Setup(x => x.ApplyMigrationsIfEnabledAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ApplyMigrationsIfEnabledAsync();

        // Assert
        _migrationServiceMock.Verify(
            x => x.ApplyMigrationsIfEnabledAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ApplyMigrationsIfEnabledAsync_When_Service_Throws_Should_Propagate_Exception()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        _migrationServiceMock
            .Setup(x => x.ApplyMigrationsIfEnabledAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ApplyMigrationsIfEnabledAsync()
        );

        Assert.Same(expectedException, exception);
        _migrationServiceMock.Verify(
            x => x.ApplyMigrationsIfEnabledAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
