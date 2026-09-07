using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Events;

[EventRouting("CourseCreated", MessageChannelType.Topic)]
public sealed record CourseCreatedEvent(
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