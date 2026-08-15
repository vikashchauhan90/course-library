namespace CourseLibrary.Application.Operations.Comments.Update;

public sealed record CommentUpdatedEvent(string CommentId, string CourseId, string AuthorId, DateTime UpdatedAt);
