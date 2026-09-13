namespace CourseLibrary.Models.Course;

public sealed record CommentResponse(
    string Id,
    string CourseId,
    string AuthorId,
    string Content,
    string? ParentCommentId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
