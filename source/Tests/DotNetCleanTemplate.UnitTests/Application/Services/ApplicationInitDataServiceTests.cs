using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Services;
using Moq;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Application.Services;

public class ApplicationInitDataServiceTests
{
    private readonly Mock<IInitDataService> _initDataServiceMock;
    private readonly ApplicationInitDataService _service;

    public ApplicationInitDataServiceTests()
    {
        _initDataServiceMock = new Mock<IInitDataService>();
        _service = new ApplicationInitDataService(_initDataServiceMock.Object);
    }

    [Fact]
    public void Constructor_Should_Initialize_Dependencies()
    {
        // Assert
        Assert.NotNull(_service);
    }

    [Fact]
    public async Task InitializeAsync_Should_Call_InitDataService()
    {
        // Arrange
        _initDataServiceMock
            .Setup(x => x.InitializeAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.InitializeAsync();

        // Assert
        _initDataServiceMock.Verify(
            x => x.InitializeAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task InitializeAsync_When_Service_Throws_Should_Propagate_Exception()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        _initDataServiceMock
            .Setup(x => x.InitializeAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.InitializeAsync()
        );

        Assert.Same(expectedException, exception);
        _initDataServiceMock.Verify(
            x => x.InitializeAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
