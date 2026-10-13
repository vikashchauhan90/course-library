using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;

namespace CourseLibrary.Course.Domain.Events;

public sealed class CourseEvent : IAuditableEntity
{
    public required CourseId CourseId { get; init; }
    public required CourseEventType EventKind { get; init; }

    public required string EventId { get; init; }

    public required string ActorId { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public IReadOnlyList<AuditEntry> ChangedProperties {  get; init; } = Array.Empty<AuditEntry>();

    public string EventType => typeof(CourseEvent).Name;
}
 