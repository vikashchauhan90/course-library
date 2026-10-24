using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.Events;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Entities;

namespace CourseLibrary.Course.EventConsumer.Consumers.Courses;

internal sealed class CourseEventPayload
{
    public string CourseId { get; init; } = string.Empty;
    public string EventKind { get; init; } = string.Empty;
    public string EventId { get; init; } = string.Empty;
    public string ActorId { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
    public IReadOnlyList<AuditEntryPayload> ChangedProperties { get; init; } = [];

    public static CourseEventPayload FromDomain(CourseEvent courseEvent)
        => new()
        {
            CourseId = courseEvent.CourseId,
            EventKind = courseEvent.EventKind,
            EventId = courseEvent.EventId,
            ActorId = courseEvent.ActorId,
            OccurredAt = courseEvent.OccurredAt,
            ChangedProperties = courseEvent.ChangedProperties
                .Select(AuditEntryPayload.FromDomain)
                .ToArray()
        };

    public CourseEvent ToDomain()
        => new()
        {
            CourseId = CourseId,
            EventKind = EventKind,
            EventId = EventId,
            ActorId = ActorId,
            OccurredAt = OccurredAt,
            ChangedProperties = ChangedProperties
                .Select(static property => property.ToDomain())
                .ToArray()
        };
}

internal sealed class AuditEntryPayload
{
    public string Action { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public object? Value { get; init; }
    public string? ValueTypeName { get; init; }

    public static AuditEntryPayload FromDomain(AuditEntry entry)
        => new()
        {
            Action = entry.Action,
            Name = entry.Name,
            Value = entry.Value,
            ValueTypeName = entry.ValueTypeName
        };

    public AuditEntry ToDomain()
        => new()
        {
            Action = Action,
            Name = Name,
            Value = Value,
            ValueTypeName = ValueTypeName
        };
}
