using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Infrastructure.Services;
using DotNetCleanTemplate.UnitTests.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Infrastructure;

public class UserLockoutCleanupServiceTests : ServiceTestBase
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IUserLockoutRepository> _userLockoutRepositoryMock;
    private readonly Mock<ILogger<UserLockoutCleanupService>> _loggerMock;
    private readonly UserLockoutCleanupSettings _settings;

    public UserLockoutCleanupServiceTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _userLockoutRepositoryMock = new Mock<IUserLockoutRepository>();
        _loggerMock = new Mock<ILogger<UserLockoutCleanupService>>();

        _settings = new UserLockoutCleanupSettings
        {
            Enabled = true,
            CleanupIntervalMinutes = 60,
            BatchSize = 100,
        };

        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_serviceScopeFactoryMock.Object);
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IUserLockoutRepository)))
            .Returns(_userLockoutRepositoryMock.Object);
    }

    [Fact]
    public async Task CleanupExpiredLockoutsAsync_WhenCalled_ShouldCallRepositoryMethod()
    {
        // Arrange
        var service = new UserLockoutCleanupService(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            Options.Create(_settings)
        );

        // Act
        await service.CleanupExpiredLockoutsAsync();

        // Assert
        _userLockoutRepositoryMock.Verify(
            x => x.ClearExpiredLockoutsAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CleanupExpiredLockoutsAsync_WhenRepositoryThrowsException_ShouldLogErrorAndNotRethrow()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        _userLockoutRepositoryMock
            .Setup(x => x.ClearExpiredLockoutsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new UserLockoutCleanupService(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            Options.Create(_settings)
        );

        // Act
        await service.CleanupExpiredLockoutsAsync();

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var service = new UserLockoutCleanupService(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            Options.Create(_settings)
        );

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new UserLockoutCleanupService(null!, _loggerMock.Object, Options.Create(_settings))
        );
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new UserLockoutCleanupService(
                _serviceProviderMock.Object,
                null!,
                Options.Create(_settings)
            )
        );
    }

    [Fact]
    public void Constructor_WithNullSettings_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new UserLockoutCleanupService(_serviceProviderMock.Object, _loggerMock.Object, null!)
        );
    }
}
