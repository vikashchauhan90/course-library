using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Entities;

public sealed record Discussion : ICosmosPartitioned
{
    public required string Id { get; init; }
    public required string CourseId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    public string PartitionKeyValue => CourseId;
}
