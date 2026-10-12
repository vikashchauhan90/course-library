namespace CourseLibrary.Course.Contracts.DTOs;

public sealed record CommentResponse(
    string Id,
    string CourseId,
    string AuthorId,
    string Content,
    string? ParentCommentId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
