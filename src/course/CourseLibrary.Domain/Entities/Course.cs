using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Entities;

[CosmosContainer("courses")]
public sealed record Course : ICosmosPartitioned
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string AuthorId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }

    public string PartitionKeyValue => AuthorId;
}
