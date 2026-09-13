using CourseLibrary.Domain.Abstractions;
using CourseLibrary.Domain.Events;
using CourseLibrary.Domain.ValueObjects;

namespace CourseLibrary.EventConsumer.Consumers.Courses;

internal sealed class CourseEventPayload
{
    public Guid CourseId { get; init; }
    public Guid EventType { get; init; }
    public string EventId { get; init; } = string.Empty;
    public string ActorId { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
    public IReadOnlyList<AuditEntryPayload> ChangedProperties { get; init; } = [];

    public static CourseEventPayload FromDomain(CourseEvent courseEvent) =>
        new()
        {
            CourseId = courseEvent.CourseId.Value,
            EventType = courseEvent.EventType.Value,
            EventId = courseEvent.EventId,
            ActorId = courseEvent.ActorId,
            OccurredAt = courseEvent.OccurredAt,
            ChangedProperties = courseEvent.ChangedProperties
                .Select(AuditEntryPayload.FromDomain)
                .ToArray()
        };

    public CourseEvent ToDomain()
    {

        return new CourseEvent
        {
            CourseId = (CourseId)CourseId,
            EventType = (CourseEventType)EventType,
            EventId = EventId,
            ActorId = ActorId,
            OccurredAt = OccurredAt,
            ChangedProperties = ChangedProperties
                .Select(property => property.ToDomain())
                .ToArray()
        };
    }
}

internal sealed class AuditEntryPayload
{
    public Guid Action { get; init; }
    public string Name { get; init; } = string.Empty;
    public object? Value { get; init; }

    public static AuditEntryPayload FromDomain(AuditEntry entry) =>
        new()
        {
            Action = entry.Action.Value,
            Name = entry.Name,
            Value = entry.Value
        };

    public AuditEntry ToDomain()
    {

        return new AuditEntry
        {
            Action = (AuditAction)Action,
            Name = Name,
            Value = Value
        };
    }
}
