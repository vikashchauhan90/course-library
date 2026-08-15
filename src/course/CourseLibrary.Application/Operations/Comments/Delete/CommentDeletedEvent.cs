namespace CourseLibrary.Application.Operations.Comments.Delete;

public sealed record CommentDeletedEvent(string CommentId, string CourseId);
