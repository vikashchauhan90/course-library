
using CourseLibrary.Domain.Abstractions;
using CourseLibrary.Domain.ValueObjects;

namespace CourseLibrary.Domain.Entities;

public sealed record Comment : IEntity<CommentId>, IAuditableEntity
{
    public required CommentId Id { get; init; }
    public required CourseId CourseId { get; init; }
    public required AuthorId AuthorId { get; init; }
    public required string Content { get; init; }
    public CommentId? ParentCommentId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
}
