using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Entities;

public sealed record Author : ICosmosPartitioned
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Bio { get; init; }
    public string? Website { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    public string PartitionKeyValue => Id;
}
