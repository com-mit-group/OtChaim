// Temporary stub for missing Yaref92.Events package to allow focusing on UserInfo functionality

using Yaref92.Events;

namespace Yaref92.Events;

/// <summary>
/// Temporary stub for missing Yaref92.Events package
/// </summary>
public abstract class DomainEventBase : IEvent
{
    public Guid Id { get; }
    public DateTime DateTimeOccurredUtc { get; }

    protected DomainEventBase() : this(DateTime.UtcNow, Guid.NewGuid())
    {
    }

    protected DomainEventBase(DateTime dateTimeOccurredUtc, Guid id)
    {
        Id = id;
        DateTimeOccurredUtc = dateTimeOccurredUtc;
    }
}