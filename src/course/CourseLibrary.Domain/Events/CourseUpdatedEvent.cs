namespace CourseLibrary.Domain.Events;

[EventRouting("CourseUpdated", MessageChannelType.Topic)]
public sealed record CourseUpdatedEvent(
    string CourseId,
    string AuthorId,
    string Title,
    string EventId,
    DateTimeOffset OccurredAt) :
    IDomainEvent;
