
using CourseLibrary.Core.Domain.Abstractions.Auditing;
using CourseLibrary.Core.Domain.Abstractions.Entities;
using CourseLibrary.Course.Domain.ValueObjects;

namespace CourseLibrary.Course.Domain.Entities;

public sealed record Comment : IEntity<CommentId>, IAuditableEntity
{
    public required CommentId Id { get; init; }
    public required CourseId CourseId { get; init; }
    public required AuthorId AuthorId { get; init; }
    public required string Content { get; init; }
    public CommentId? ParentCommentId { get; init; }
    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
