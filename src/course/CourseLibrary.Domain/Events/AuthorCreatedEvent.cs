namespace CourseLibrary.Domain.Events;

[EventRouting("AuthorCreated", MessageChannelType.Topic)]
public sealed record AuthorCreatedEvent(
    string AuthorId,
    string Name,
    string EventId,
    DateTimeOffset OccurredAt) :
    IDomainEvent;