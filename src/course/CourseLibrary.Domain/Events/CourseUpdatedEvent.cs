namespace CourseLibrary.Domain.Events;

public sealed record CourseUpdatedEvent(
    string CourseId,
    string AuthorId,
    string Title,
    string EventId,
    DateTimeOffset OccurredAt) :
    IDomainEvent;
