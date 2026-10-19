namespace CourseLibrary.Core.Domain.Abstractions.Events;

public interface IDomainEvent
{
    string EventId { get; }
    string? ActorId { get; }
    DateTimeOffset OccurredAt { get; }
    string EventType { get; }
}