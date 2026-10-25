using CourseLibrary.Course.Contracts.DTOs.Courses.Audits;

namespace CourseLibrary.Course.EventConsumer.Consumers.Courses;

internal sealed class CourseEventPayload
{
    public string CourseId { get; init; } = string.Empty;
    public string EventKind { get; init; } = string.Empty;
    public string EventId { get; init; } = string.Empty;
    public string ActorId { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
    public IReadOnlyList<AuditEntry> ChangedProperties { get; init; } = [];

    public static CourseEventPayload FromRequest(CourseEventRequest courseEvent)
        => new()
        {
            CourseId = courseEvent.CourseId,
            EventKind = courseEvent.EventKind,
            EventId = courseEvent.EventId,
            ActorId = courseEvent.ActorId,
            OccurredAt = courseEvent.OccurredAt,
            ChangedProperties = courseEvent.ChangedProperties
        };

    public CourseEventRequest ToRequest()
        => new()
        {
            CourseId = CourseId,
            EventKind = EventKind,
            EventId = EventId,
            ActorId = ActorId,
            OccurredAt = OccurredAt,
            ChangedProperties = ChangedProperties
        };
}
