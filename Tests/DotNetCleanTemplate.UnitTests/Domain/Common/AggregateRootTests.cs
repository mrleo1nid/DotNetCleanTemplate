using DotNetCleanTemplate.Domain.Common;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Domain.Common;

public class TestAggregateRoot : AggregateRoot<Guid>
{
    public TestAggregateRoot() { }

    public TestAggregateRoot(Guid id)
    {
        Id = id;
    }
}

public class AggregateRootTests
{
    [Fact]
    public void Constructor_ShouldInitializeDomainEvents()
    {
        // Act
        var aggregate = new TestAggregateRoot();

        // Assert
        Assert.NotNull(aggregate.DomainEvents);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void AddDomainEvent_ShouldAddEventToList()
    {
        // Arrange
        var aggregate = new TestAggregateRoot();
        var domainEvent = new TestDomainEvent();

        // Act
        aggregate.AddDomainEvent(domainEvent);

        // Assert
        Assert.Single(aggregate.DomainEvents);
        Assert.Contains(domainEvent, aggregate.DomainEvents);
    }

    [Fact]
    public void RemoveDomainEvent_ShouldRemoveEventFromList()
    {
        // Arrange
        var aggregate = new TestAggregateRoot();
        var domainEvent = new TestDomainEvent();
        aggregate.AddDomainEvent(domainEvent);

        // Act
        aggregate.RemoveDomainEvent(domainEvent);

        // Assert
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_ShouldClearAllEvents()
    {
        // Arrange
        var aggregate = new TestAggregateRoot();
        aggregate.AddDomainEvent(new TestDomainEvent());
        aggregate.AddDomainEvent(new TestDomainEvent());

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void DomainEvents_ShouldReturnEventsList()
    {
        // Arrange
        var aggregate = new TestAggregateRoot();
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();
        aggregate.AddDomainEvent(event1);
        aggregate.AddDomainEvent(event2);

        // Act
        var events = aggregate.DomainEvents;

        // Assert
        Assert.Equal(2, events.Count);
        Assert.Contains(event1, events);
        Assert.Contains(event2, events);
    }
}

public class TestDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
