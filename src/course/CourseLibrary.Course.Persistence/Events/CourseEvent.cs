using CourseLibrary.Course.Persistence.Entities;
using CourseLibrary.Course.Contracts.DTOs.Courses.Audits;

namespace CourseLibrary.Course.Persistence.Events;

internal sealed class CourseEvent : IDomainEvent
{
    public required string CourseId { get; init; }
    public required string EventKind { get; init; }

    public required string EventId { get; init; }

    public required string ActorId { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public IReadOnlyList<AuditEntry> ChangedProperties {  get; init; } = Array.Empty<AuditEntry>();

    public string EventType => typeof(CourseEvent).Name;
}
 