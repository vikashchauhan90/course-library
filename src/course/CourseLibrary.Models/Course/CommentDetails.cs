namespace CourseLibrary.Models.Course;

public sealed record CommentDetails(
    string Id,
    string CourseId,
    string AuthorId,
    string Content,
    string? ParentCommentId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
