namespace CourseLibrary.Domain.Events;

[EventRouting("CourseCreated", MessageChannelType.Topic)]
public sealed record CourseCreatedEvent(
    string CourseId,
    string AuthorId,
    string Title,
    string Description,
    string EventId,
    string ActorId,
    DateTimeOffset OccurredAt) :
    IDomainEvent;