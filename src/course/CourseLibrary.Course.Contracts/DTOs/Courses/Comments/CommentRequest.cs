namespace CourseLibrary.Course.Contracts.DTOs.Courses.Comments;

public sealed record CommentRequest(
    string? CourseId,
    string? AuthorId,
    string? Content,
    string? ParentCommentId);