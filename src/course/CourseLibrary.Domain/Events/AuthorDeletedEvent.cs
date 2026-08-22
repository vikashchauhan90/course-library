namespace CourseLibrary.Domain.Events;

[EventRouting("AuthorDeleted", MessageChannelType.Topic)]
public sealed record AuthorDeletedEvent(
    string AuthorId,
    string EventId,
    DateTimeOffset OccurredAt) :
    IDomainEvent;
