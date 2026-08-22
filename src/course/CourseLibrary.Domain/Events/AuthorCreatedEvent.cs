namespace CourseLibrary.Domain.Events;

[EventRouting("AuthorCreated", MessageChannelType.Topic)]
public sealed record AuthorCreatedEvent(
    string AuthorId,
    string Name,
    string? Bio,
    string? Website,
    string EventId,
    string ActorName,
    DateTimeOffset OccurredAt) :
    IDomainEvent;