namespace CourseLibrary.Api.Endpoints.Comments.CreateComment;

public sealed record CreateCommentRequest(string CourseId, string AuthorId, string Content, string? ParentCommentId);
