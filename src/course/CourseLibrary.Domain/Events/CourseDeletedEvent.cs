namespace CourseLibrary.Domain.Events;

[EventRouting("CourseDeleted", MessageChannelType.Topic)]
public sealed record CourseDeletedEvent(
    string CourseId,
    string PartitionKey,
    string EventId,
    DateTimeOffset OccurredAt) :
    IDomainEvent;