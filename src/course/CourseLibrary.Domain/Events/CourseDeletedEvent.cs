namespace CourseLibrary.Domain.Events;

[EventRouting("CourseDeleted", MessageChannelType.Topic)]
public sealed record CourseDeletedEvent(
    string CourseId,
    string EventId,
    string ActorId,
    DateTimeOffset OccurredAt) :
    IDomainEvent;