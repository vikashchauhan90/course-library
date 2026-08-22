namespace CourseLibrary.Application.Operations.Comments;

/// <summary>
/// Response model for Comment operations.
/// </summary>
public sealed record CommentResponse(
    string Id,
    string CourseId,
    string AuthorId,
    string Content,
    string? ParentCommentId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
