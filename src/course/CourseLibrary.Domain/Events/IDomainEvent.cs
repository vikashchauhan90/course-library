
namespace CourseLibrary.Domain.Events;

public interface IDomainEvent
{
    string EventId { get; }
    DateTimeOffset OccurredAt { get; }
    string EventType => GetType().Name;
}