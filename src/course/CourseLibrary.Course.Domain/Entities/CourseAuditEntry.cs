using CourseLibrary.Core.Domain.Abstractions.Auditing;
using CourseLibrary.Core.Domain.Abstractions.Entities;

namespace CourseLibrary.Course.Domain.Entities;

public sealed record CourseAuditEntry : IEntity<string>, IAuditableEntity
{
    public required string Id { get; init; }
    public required string CourseId { get; init; }
    public required string PartitionKey { get; init; }
    public required string Action { get; init; }
    public string? EventId { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public IReadOnlyList<AuditEntry>? ChangedProperties { get; init; }
    public string? ActorId { get; init; }
    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
