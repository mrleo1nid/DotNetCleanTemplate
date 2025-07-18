using DotNetCleanTemplate.Application.Behaviors;
using DotNetCleanTemplate.Application.Caching;
using DotNetCleanTemplate.Domain.Services;
using MediatR;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Application
{
    public class CachingBehaviorTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly RequestHandlerDelegate<string> _next;

        public CachingBehaviorTests()
        {
            _cacheServiceMock = new Mock<ICacheService>();
            _next = (ct) => Task.FromResult("handled");
        }

        [Fact]
        public async Task Handle_UsesCacheAttribute()
        {
            var behavior = new CachingBehavior<CachedRequest, string>(_cacheServiceMock.Object);
            _cacheServiceMock
                .Setup(x =>
                    x.GetOrCreateAsync<string>(
                        "key",
                        "region",
                        It.IsAny<Func<Task<string>>>(),
                        CancellationToken.None
                    )
                )
                .ReturnsAsync("cached");
            var result = await behavior.Handle(new CachedRequest(), _next, CancellationToken.None);
            Assert.Equal("cached", result);
            _cacheServiceMock.Verify(
                x =>
                    x.GetOrCreateAsync<string>(
                        "key",
                        "region",
                        It.IsAny<Func<Task<string>>>(),
                        CancellationToken.None
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_UsesInvalidateCacheAttribute_Key()
        {
            var behavior = new CachingBehavior<InvalidateKeyRequest, string>(
                _cacheServiceMock.Object
            );
            var result = await behavior.Handle(
                new InvalidateKeyRequest(),
                _next,
                CancellationToken.None
            );
            Assert.Equal("handled", result);
            _cacheServiceMock.Verify(x => x.Invalidate("key"), Times.Once);
        }

        [Fact]
        public async Task Handle_UsesInvalidateCacheAttribute_Region()
        {
            var behavior = new CachingBehavior<InvalidateRegionRequest, string>(
                _cacheServiceMock.Object
            );
            var result = await behavior.Handle(
                new InvalidateRegionRequest(),
                _next,
                CancellationToken.None
            );
            Assert.Equal("handled", result);
            _cacheServiceMock.Verify(x => x.InvalidateRegion("region"), Times.Once);
        }

        [Fact]
        public async Task Handle_NoAttributes_CallsNext()
        {
            var behavior = new CachingBehavior<NoAttrRequest, string>(_cacheServiceMock.Object);
            var result = await behavior.Handle(new NoAttrRequest(), _next, CancellationToken.None);
            Assert.Equal("handled", result);
            _cacheServiceMock.VerifyNoOtherCalls();
        }

        [Cache("key", Region = "region")]
        private class CachedRequest : IRequest<string> { }

        [InvalidateCache(Key = "key")]
        private class InvalidateKeyRequest : IRequest<string> { }

        [InvalidateCache(Region = "region")]
        private class InvalidateRegionRequest : IRequest<string> { }

        private class NoAttrRequest : IRequest<string> { }
    }
}
