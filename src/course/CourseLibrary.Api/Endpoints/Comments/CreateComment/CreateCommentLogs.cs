namespace CourseLibrary.Api.Endpoints.Comments.CreateComment;

public static partial class CreateCommentLogs
{
    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Information,
        Message = "Creating comment from author {AuthorId}")]
    public static partial void CreatingComment(
        this ILogger logger,
        string authorId);

    [LoggerMessage(
        EventId = 4005,
        Level = LogLevel.Information,
        Message = "Comment {CommentId} created")]
    public static partial void CommentCreated(
        this ILogger logger,
        string commentId);
}
