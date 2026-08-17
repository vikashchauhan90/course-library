namespace CourseLibrary.Api.Endpoints.Comments.DeleteComment;

public static partial class DeleteCommentLogs
{
    [LoggerMessage(
        EventId = 4008,
        Level = LogLevel.Information,
        Message = "Deleting comment {CommentId}")]
    public static partial void DeletingComment(
        this ILogger logger,
        string commentId);

    [LoggerMessage(
        EventId = 4009,
        Level = LogLevel.Information,
        Message = "Comment {CommentId} deleted")]
    public static partial void CommentDeleted(
        this ILogger logger,
        string commentId);

    [LoggerMessage(
        EventId = 4010,
        Level = LogLevel.Warning,
        Message = "Comment {CommentId} not found for deletion")]
    public static partial void CommentNotFoundForDeletion(
        this ILogger logger,
        string commentId);
}
