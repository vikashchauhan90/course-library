using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Events;

[EventRouting("CourseUpdated", MessageChannelType.Topic)]
public sealed record CourseUpdatedEvent(
    string CourseId,
    string EventId,
    string ActorId,
    DateTimeOffset OccurredAt,
    IReadOnlyList<AuditEntry> ChangedProperties
) : AuditableDomainEvent(
    EventId,
    ActorId,
    OccurredAt,
    ChangedProperties);
