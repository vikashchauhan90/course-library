using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Events;

[EventRouting("CourseCreated", MessageChannelType.Topic)]
public sealed record CourseCreatedEvent(
    string CourseId,
    string AuthorId,
    string Title,
    string Description,
    string AuthorName,
    string EventId,
    string ActorId,
    DateTimeOffset OccurredAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? RetiredAt,
    DateTimeOffset? DeletedAt,
    IReadOnlyList<string> ChangedProperties) :
    IDomainEvent;