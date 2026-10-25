namespace CourseLibrary.Course.Contracts.DTOs.Courses.Audits;

public record CourseAuditResponse
{
    public required string Id { get; init; }
    public required string CourseId { get; init; }
    public required string Action { get; init; }
    public string? EventId { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public IReadOnlyList<AuditEntry>? ChangedProperties { get; init; }
    public string? ActorId { get; init; }
}
