namespace CourseLibrary.Domain.Events;

public sealed record AuthorDeletedEvent(string AuthorId, string EventId, DateTimeOffset OccurredAt) :
    IDomainEvent;
