using CourseLibrary.Course.Domain.Abstractions;
using CourseLibrary.Course.Domain.ValueObjects;

namespace CourseLibrary.Course.Domain.Events;

public sealed class CourseEvent : IDomainEvent
{
    public required CourseId CourseId { get; init; }
    public required CourseEventType EventType { get; init; }

    public required string EventId { get; init; }

    public required string ActorId { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public IReadOnlyList<AuditEntry> ChangedProperties {  get; init; } = Array.Empty<AuditEntry>();
}
 