namespace CourseLibrary.Domain.Events;

public sealed record AuthorCreatedEvent(
    string AuthorId,
    string Name,
    string EventId,
    DateTimeOffset OccurredAt) :
    IDomainEvent;