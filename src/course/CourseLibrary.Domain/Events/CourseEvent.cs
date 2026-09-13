using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Events;

public sealed class CourseEvent : AuditableDomainEvent
{
    public required string CourseId { get; init; }
    public required Guid EventType { get; init; }
}
 