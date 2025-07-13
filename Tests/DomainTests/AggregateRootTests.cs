using System;
using DotNetCleanTemplate.Domain.Common;
using Xunit;

namespace DomainTests
{
    public class DummyDomainEvent : IDomainEvent { }

    public class DummyAggregateRoot : AggregateRoot<Guid>
    {
        public DummyAggregateRoot(Guid id)
        {
            Id = id;
        }
    }

    public class AggregateRootTests
    {
        [Fact]
        public void AddDomainEvent_AddsEvent()
        {
            var aggregate = new DummyAggregateRoot(Guid.NewGuid());
            var domainEvent = new DummyDomainEvent();
            aggregate.AddDomainEvent(domainEvent);
            Assert.Contains(domainEvent, aggregate.DomainEvents);
        }

        [Fact]
        public void RemoveDomainEvent_RemovesEvent()
        {
            var aggregate = new DummyAggregateRoot(Guid.NewGuid());
            var domainEvent = new DummyDomainEvent();
            aggregate.AddDomainEvent(domainEvent);
            aggregate.RemoveDomainEvent(domainEvent);
            Assert.DoesNotContain(domainEvent, aggregate.DomainEvents);
        }

        [Fact]
        public void ClearDomainEvents_RemovesAllEvents()
        {
            var aggregate = new DummyAggregateRoot(Guid.NewGuid());
            aggregate.AddDomainEvent(new DummyDomainEvent());
            aggregate.AddDomainEvent(new DummyDomainEvent());
            aggregate.ClearDomainEvents();
            Assert.Empty(aggregate.DomainEvents);
        }
    }
}
