using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Entities;

public sealed record CourseAuditEntry : IEntity
{
    public required string Id { get; init; }
    public required string CourseId { get; init; }
    public required Guid Action { get; init; }
    public string? EventId { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public IReadOnlyList<AuditEntry>? ChangedProperties { get; init; }
    public string? ActorId { get; init; }
}
