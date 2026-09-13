using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Events;

public sealed class CourseEvent : IDomainEvent
{
    public required string CourseId { get; init; }
    public required Guid EventType { get; init; }

    public required string EventId { get; init; }

    public required string ActorId { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public IReadOnlyList<AuditEntry> ChangedProperties {  get; init; } = Array.Empty<AuditEntry>();
}
 