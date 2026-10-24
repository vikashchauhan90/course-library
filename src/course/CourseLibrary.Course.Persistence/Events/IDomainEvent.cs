using CourseLibrary.Course.Persistence.Entities;

namespace CourseLibrary.Course.Persistence.Events;

internal interface IDomainEvent
{
    string EventId { get; }
    string? ActorId { get; }
    DateTimeOffset OccurredAt { get; }
    string EventType { get; }
    IReadOnlyList<AuditEntry> ChangedProperties { get; }
}