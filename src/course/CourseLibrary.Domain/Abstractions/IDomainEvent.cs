namespace CourseLibrary.Domain.Abstractions;

public interface IDomainEvent
{
    string EventId { get; }
    string ActorId { get; }
    DateTimeOffset OccurredAt { get; }
    IReadOnlyList<AuditEntry> ChangedProperties { get; }
    string EventType => GetType().Name;    
}