using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Entities;

[CosmosContainer("comments")]
public sealed record Comment : ICosmosPartitioned
{
    public required string Id { get; init; }
    public required string CourseId { get; init; }
    public required string AuthorId { get; init; }
    public required string Content { get; init; }
    public string? ParentCommentId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }

    public string PartitionKeyValue => CourseId;
}
