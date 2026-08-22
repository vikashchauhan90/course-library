namespace CourseLibrary.Domain.Events;

[EventRouting("AuthorUpdated", MessageChannelType.Topic)]
public sealed record AuthorUpdatedEvent(
    string AuthorId,
    string Name,
    string? Bio,
    string? Website,
    string EventId,
    string ActorId,
    DateTimeOffset OccurredAt) :
    IDomainEvent;