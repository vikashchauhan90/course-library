namespace CourseLibrary.Course.Contracts.DTOs.Courses.Comments;

public sealed record CommentResponse(
    string Id,
    string CourseId,
    string AuthorId,
    string Content,
    string? ParentCommentId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? DeletedAt);
