namespace CourseLibrary.Course.Persistence.Entities;

internal sealed record Comment : IEntity<string>
{
    public required string Id { get; init; }
    public required string CourseId { get; init; }
    public required string PartitionKey { get; init; }
    public required string AuthorId { get; init; }
    public required string Content { get; init; }
    public string? ParentCommentId { get; init; }
    public string? ConcurrencyStamp { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
