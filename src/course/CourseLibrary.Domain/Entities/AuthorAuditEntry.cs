using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Entities;

[CosmosContainer("author-audit")]
public sealed record AuthorAuditEntry : ICosmosPartitioned
{
    public required string Id { get; init; }
    public required string AuthorId { get; init; }
    public required AuditAction Action { get; init; }
    public required string Name { get; init; }
    public string? Bio { get; init; }
    public string? Website { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public string? ActorId { get; init; }
    public string? CorrelationId { get; init; }
    public string PartitionKeyValue => AuthorId;
}
