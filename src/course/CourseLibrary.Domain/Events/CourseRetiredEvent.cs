using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Events;

[EventRouting("CourseRetired", MessageChannelType.Topic)]
public sealed record CourseRetiredEvent(
    string CourseId,
    string EventId,
    string ActorId,
    DateTimeOffset OccurredAt,
    IReadOnlyList<AuditEntry> ChangedProperties)
    : AuditableDomainEvent(EventId, ActorId, OccurredAt, ChangedProperties);