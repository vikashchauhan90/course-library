using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Events;

[EventRouting("CourseUpdated", MessageChannelType.Topic)]
public sealed record CourseUpdatedEvent(
    string CourseId,
    string AuthorId,
    string Title,
    string? Description,
    string AuthorName,
    string EventId,
    string ActorId,
    DateTimeOffset OccurredAt) :
    IDomainEvent;
