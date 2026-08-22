namespace CourseLibrary.Domain.Events;

[EventRouting("AuthorUpdated", MessageChannelType.Topic)]
public sealed record AuthorUpdatedEvent(
    string AuthorId,
    string Name,
    string EventId,
    DateTimeOffset OccurredAt) :
    IDomainEvent;