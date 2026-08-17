namespace CourseLibrary.Api.Endpoints.Comments.GetComment;

public static partial class GetCommentLogs
{
    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Information,
        Message = "Getting comment {CommentId}")]
    public static partial void GettingComment(
        this ILogger logger,
        string commentId);

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Information,
        Message = "Comment {CommentId} retrieved")]
    public static partial void CommentRetrieved(
        this ILogger logger,
        string commentId);

    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Warning,
        Message = "Comment {CommentId} not found")]
    public static partial void CommentNotFound(
        this ILogger logger,
        string commentId);
}
