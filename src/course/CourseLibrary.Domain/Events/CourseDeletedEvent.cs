using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Events;

[EventRouting("CourseDeleted", MessageChannelType.Topic)]
public sealed record CourseDeletedEvent(
    string CourseId,
    string EventId,
    string ActorId,
    DateTimeOffset OccurredAt,
    IReadOnlyList<AuditEntry> ChangedProperties)
    : AuditableDomainEvent(
    EventId,
    ActorId,
    OccurredAt,
    ChangedProperties);