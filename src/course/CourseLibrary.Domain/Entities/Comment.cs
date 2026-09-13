
using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Entities;

public sealed record Comment : IEntity<string>, IAuditableEntity
{
    public required string Id { get; init; }
    public required string CourseId { get; init; }
    public required string AuthorId { get; init; }
    public required string Content { get; init; }
    public string? ParentCommentId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
}
