using CourseLibrary.Domain.Abstractions;
using CourseLibrary.Domain.ValueObjects;

namespace CourseLibrary.Domain.Entities;

public sealed record CourseAuditEntry : IEntity<string>, IAuditableEntity
{
    public required string Id { get; init; }
    public required CourseId CourseId { get; init; }
    public required Guid Action { get; init; }
    public string? EventId { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public IReadOnlyList<AuditEntry>? ChangedProperties { get; init; }
    public string? ActorId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
}
