namespace CourseLibrary.Course.Contracts.DTOs.Courses.Audits;

public sealed record CourseEventRequest
{
    public required string CourseId { get; init; }
    public required string EventKind { get; init; }
    public required string EventId { get; init; }
    public required string ActorId { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public IReadOnlyList<AuditEntry> ChangedProperties { get; init; } = [];
    public string EventType => "CourseEvent";
}
