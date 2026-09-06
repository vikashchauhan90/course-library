namespace CourseLibrary.Domain.Abstractions;

public interface IDomainEvent
{
    string EventId { get; }
    DateTimeOffset OccurredAt { get; }
    string EventType => GetType().Name;
}