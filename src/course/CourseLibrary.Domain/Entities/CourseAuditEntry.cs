using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Entities;

[CosmosContainer("course-audit")]
public sealed record CourseAuditEntry : ICosmosPartitioned
{
    public required string Id { get; init; }
    public required string CourseId { get; init; }
    public required AuditAction Action { get; init; }
    public required string AuthorId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public string? ActorId { get; init; }
    public string PartitionKeyValue => CourseId;
}
