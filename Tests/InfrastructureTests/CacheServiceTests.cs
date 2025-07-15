using CacheManager.Core;
using DotNetCleanTemplate.Infrastructure.Services;
using Moq;

namespace InfrastructureTests
{
    public class CacheServiceTests
    {
        private readonly Mock<ICache<object>> _cacheMock;
        private readonly CacheService _cacheService;

        public CacheServiceTests()
        {
            _cacheMock = new Mock<ICache<object>>();
            _cacheService = new CacheService(_cacheMock.Object);
        }

        [Fact]
        public async Task GetOrCreateAsync_ReturnsFromCache_IfExists()
        {
            _cacheMock.Setup(c => c.Get<string>("key")).Returns("cached");
            var result = await _cacheService.GetOrCreateAsync<string>(
                "key",
                null,
                () => Task.FromResult("factory")
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
                () => Task.FromResult("factory")
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
                () => Task.FromResult("factory")
            );
            Assert.Equal("factory", result);
            _cacheMock.Verify(c => c.Put("key", "factory", "region"), Times.Once);
        }

        [Fact]
        public void InvalidateAsync_RemovesKey()
        {
            _cacheService.InvalidateAsync("key");
            _cacheMock.Verify(c => c.Remove("key"), Times.Once);
        }

        [Fact]
        public void InvalidateRegionAsync_ClearsRegion()
        {
            _cacheService.InvalidateRegionAsync("region");
            _cacheMock.Verify(c => c.ClearRegion("region"), Times.Once);
        }

        [Fact]
        public async Task GetOrCreateAsync_ThrowsIfKeyNull()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _cacheService.GetOrCreateAsync<string>(default!, null, () => Task.FromResult("x"))
            );
        }

        [Fact]
        public async Task GetOrCreateAsync_ThrowsIfFactoryNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _cacheService.GetOrCreateAsync<string>("key", null, null!)
            );
        }

        [Fact]
        public void InvalidateAsync_ThrowsIfKeyNull()
        {
            Assert.Throws<ArgumentException>(() => _cacheService.InvalidateAsync(default!));
        }

        [Fact]
        public void InvalidateRegionAsync_ThrowsIfRegionNull()
        {
            Assert.Throws<ArgumentException>(() => _cacheService.InvalidateRegionAsync(default!));
        }
    }
}
