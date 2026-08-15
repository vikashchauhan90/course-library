namespace CourseLibrary.Application.Operations.Comments.Create;

public sealed record CommentCreatedEvent(string CommentId, string CourseId, string AuthorId, DateTime CreatedAt);
