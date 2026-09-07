using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Events;

[EventRouting("CourseDeleted", MessageChannelType.Topic)]
public sealed record CourseDeletedEvent(
    string CourseId,
    string AuthorId,
    string AuthorName,
    string Title,
    string Description,
    string EventId,
    string ActorId,
    DateTimeOffset OccurredAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? RetiredAt,
    DateTimeOffset? DeletedAt,
    IReadOnlyList<string> ChangedProperties) :
    IDomainEvent;