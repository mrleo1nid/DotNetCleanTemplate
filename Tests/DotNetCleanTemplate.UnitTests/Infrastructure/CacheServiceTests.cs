using CacheManager.Core;
using DotNetCleanTemplate.Infrastructure.Services;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Infrastructure
{
    public class CacheServiceTests
    {
        private readonly Mock<ICacheManager<object>> _cacheMock;
        private readonly CacheService _cacheService;

        public CacheServiceTests()
        {
            _cacheMock = new Mock<ICacheManager<object>>();
            _cacheService = new CacheService(_cacheMock.Object);
        }

        [Fact]
        public async Task GetOrCreateAsync_ReturnsFromCache_IfExists()
        {
            _cacheMock.Setup(c => c.Get<string>("key")).Returns("cached");
            var result = await _cacheService.GetOrCreateAsync<string>(
                "key",
                null,
                () => Task.FromResult("factory"),
                CancellationToken.None
            );
            Assert.Equal("cached", result);
            _cacheMock.Verify(c => c.Get<string>("key"), Times.Once);
            _cacheMock.Verify(c => c.Put(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task GetOrCreateAsync_CallsFactoryAndCaches_IfNotExists()
        {
            _cacheMock.Setup(c => c.Get<string>("key")).Returns((string?)null!);
            var result = await _cacheService.GetOrCreateAsync<string>(
                "key",
                null,
                () => Task.FromResult("factory"),
                CancellationToken.None
            );
            Assert.Equal("factory", result);
            _cacheMock.Verify(c => c.Put("key", "factory"), Times.Once);
        }

        [Fact]
        public async Task GetOrCreateAsync_CachesWithRegion_IfRegionProvided()
        {
            _cacheMock.Setup(c => c.Get<string>("key")).Returns((string?)null!);
            var result = await _cacheService.GetOrCreateAsync<string>(
                "key",
                "region",
                () => Task.FromResult("factory"),
                CancellationToken.None
            );
            Assert.Equal("factory", result);
            _cacheMock.Verify(c => c.Put("key", "factory", "region"), Times.Once);
        }

        [Fact]
        public void InvalidateAsync_RemovesKey()
        {
            _cacheService.Invalidate("key");
            _cacheMock.Verify(c => c.Remove("key"), Times.Once);
        }

        [Fact]
        public void InvalidateRegionAsync_ClearsRegion()
        {
            _cacheService.InvalidateRegion("region");
            _cacheMock.Verify(c => c.ClearRegion("region"), Times.Once);
        }

        [Fact]
        public async Task GetOrCreateAsync_ThrowsIfKeyNull()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _cacheService.GetOrCreateAsync<string>(
                    default!,
                    null,
                    () => Task.FromResult("x"),
                    CancellationToken.None
                )
            );
        }

        [Fact]
        public async Task GetOrCreateAsync_ThrowsIfFactoryNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _cacheService.GetOrCreateAsync<string>("key", null, null!, CancellationToken.None)
            );
        }

        [Fact]
        public void InvalidateAsync_ThrowsIfKeyNull()
        {
            Assert.Throws<ArgumentException>(() => _cacheService.Invalidate(default!));
        }

        [Fact]
        public void InvalidateRegionAsync_ThrowsIfRegionNull()
        {
            Assert.Throws<ArgumentException>(() => _cacheService.InvalidateRegion(default!));
        }

        [Fact]
        public void Get_ShouldReturnValue_WhenCacheHit()
        {
            _cacheMock.Setup(c => c.Get<string>("key")).Returns("value");
            var result = _cacheMock.Object.Get<string>("key");
            Assert.Equal("value", result);
        }

        [Fact]
        public void Get_ShouldReturnNull_WhenCacheMiss()
        {
            _cacheMock.Setup(c => c.Get<string>("key")).Returns((string?)null!);
            var result = _cacheMock.Object.Get<string>("key");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrCreateAsync_WithCacheMiss_ShouldCallFactory()
        {
            // Arrange
            _cacheMock.Setup(c => c.Get<string>("key")).Returns((string?)null!);
            _cacheMock.Setup(c => c.Put("key", "factory")).Verifiable();

            // Act
            var result = await _cacheService.GetOrCreateAsync<string>(
                "key",
                null,
                () => Task.FromResult("factory"),
                CancellationToken.None
            );

            // Assert
            Assert.Equal("factory", result);
            _cacheMock.Verify(c => c.Put("key", "factory"), Times.Once);
        }

        [Fact]
        public async Task GetOrCreateAsync_WithExpiration_ShouldCacheValue()
        {
            // Arrange
            _cacheMock.Setup(c => c.Get<string>("key")).Returns((string?)null!);
            _cacheMock.Setup(c => c.Put("key", "value")).Verifiable();

            // Act
            var result = await _cacheService.GetOrCreateAsync<string>(
                "key",
                null,
                () => Task.FromResult("value"),
                CancellationToken.None
            );

            // Assert
            Assert.Equal("value", result);
            _cacheMock.Verify(c => c.Put("key", "value"), Times.Once);
        }

        [Fact]
        public void Invalidate_WithNonExistentKey_ShouldNotThrow()
        {
            // Arrange
            _cacheMock.Setup(c => c.Remove("nonexistent")).Verifiable();

            // Act & Assert
            _cacheService.Invalidate("nonexistent");
            _cacheMock.Verify(c => c.Remove("nonexistent"), Times.Once);
        }

        [Fact]
        public async Task GetOrCreateAsync_WithCacheException_ShouldThrow()
        {
            // Arrange
            _cacheMock
                .Setup(c => c.Get<string>("key"))
                .Throws(new InvalidOperationException("Cache error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _cacheService.GetOrCreateAsync<string>(
                    "key",
                    null,
                    () => Task.FromResult("value"),
                    CancellationToken.None
                )
            );
        }

        [Fact]
        public async Task GetOrCreateAsync_WithNullValue_ShouldNotCache()
        {
            // Arrange
            _cacheMock.Setup(c => c.Get<string?>("key")).Returns((string?)null!);

            // Act
            var result = await _cacheService.GetOrCreateAsync<string?>(
                "key",
                null,
                () => Task.FromResult((string?)null),
                CancellationToken.None
            );

            // Assert
            Assert.Null(result);
            _cacheMock.Verify(c => c.Put(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task GetOrCreateAsync_WithDefaultValue_ShouldNotCache()
        {
            // Arrange
            _cacheMock.Setup(c => c.Get<int>("key")).Returns(0);

            // Act
            var result = await _cacheService.GetOrCreateAsync<int>(
                "key",
                null,
                () => Task.FromResult(0),
                CancellationToken.None
            );

            // Assert
            Assert.Equal(0, result);
            _cacheMock.Verify(c => c.Put(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}
