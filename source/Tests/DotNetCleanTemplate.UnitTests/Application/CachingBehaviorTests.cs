using DotNetCleanTemplate.Application.Behaviors;
using DotNetCleanTemplate.Application.Caching;
using DotNetCleanTemplate.Domain.Services;
using MediatR;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Application
{
    public class CachingBehaviorTests
    {
        private readonly Mock<ICacheReader> _cacheReaderMock;
        private readonly Mock<ICacheInvalidator> _cacheInvalidatorMock;
        private readonly RequestHandlerDelegate<string> _next;

        public CachingBehaviorTests()
        {
            _cacheReaderMock = new Mock<ICacheReader>();
            _cacheInvalidatorMock = new Mock<ICacheInvalidator>();
            _next = (ct) => Task.FromResult("handled");
        }

        [Fact]
        public async Task Handle_UsesCacheAttribute()
        {
            var behavior = new CachingBehavior<CachedRequest, string>(
                _cacheReaderMock.Object,
                _cacheInvalidatorMock.Object
            );
            _cacheReaderMock
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
            _cacheReaderMock.Verify(
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
                _cacheReaderMock.Object,
                _cacheInvalidatorMock.Object
            );
            var result = await behavior.Handle(
                new InvalidateKeyRequest(),
                _next,
                CancellationToken.None
            );
            Assert.Equal("handled", result);
            _cacheInvalidatorMock.Verify(x => x.Invalidate("key"), Times.Once);
        }

        [Fact]
        public async Task Handle_UsesInvalidateCacheAttribute_Region()
        {
            var behavior = new CachingBehavior<InvalidateRegionRequest, string>(
                _cacheReaderMock.Object,
                _cacheInvalidatorMock.Object
            );
            var result = await behavior.Handle(
                new InvalidateRegionRequest(),
                _next,
                CancellationToken.None
            );
            Assert.Equal("handled", result);
            _cacheInvalidatorMock.Verify(x => x.InvalidateRegion("region"), Times.Once);
        }

        [Fact]
        public async Task Handle_NoAttributes_CallsNext()
        {
            var behavior = new CachingBehavior<NoAttrRequest, string>(
                _cacheReaderMock.Object,
                _cacheInvalidatorMock.Object
            );
            var result = await behavior.Handle(new NoAttrRequest(), _next, CancellationToken.None);
            Assert.Equal("handled", result);
            _cacheReaderMock.VerifyNoOtherCalls();
            _cacheInvalidatorMock.VerifyNoOtherCalls();
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
