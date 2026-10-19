namespace CourseLibrary.Core.Domain.Abstractions.Events;

public abstract record DomainEvent : IDomainEvent
{
    public string EventId { get; init; } =
       Guid.NewGuid().ToString();

    public DateTimeOffset OccurredAt { get; init; } =
        DateTimeOffset.UtcNow;

    public virtual string? ActorId => null;
    public virtual string EventType => GetType().Name;
}
