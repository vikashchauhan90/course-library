namespace CourseLibrary.Api.Endpoints.Comments.UpdateComment;

public static partial class UpdateCommentLogs
{
    [LoggerMessage(
        EventId = 4006,
        Level = LogLevel.Information,
        Message = "Updating comment {CommentId}")]
    public static partial void UpdatingComment(
        this ILogger logger,
        string commentId);

    [LoggerMessage(
        EventId = 4007,
        Level = LogLevel.Information,
        Message = "Comment {CommentId} updated")]
    public static partial void CommentUpdated(
        this ILogger logger,
        string commentId);
}
