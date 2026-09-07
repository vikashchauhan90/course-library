namespace CourseLibrary.Domain.Abstractions;

public abstract record AuditableDomainEvent(
    string EventId,
    string ActorId,
    DateTimeOffset OccurredAt,
    IReadOnlyList<AuditEntry> ChangedProperties
) : IDomainEvent;
