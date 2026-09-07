namespace CourseLibrary.Domain.Abstractions;

public abstract class AuditableDomainEvent: IDomainEvent
{
    public required string EventId { get; init; }
    public required string ActorId { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public IReadOnlyList<AuditEntry> ChangedProperties { get; init; } = Array.Empty<AuditEntry>();
}
