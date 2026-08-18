using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Comments;

internal static partial class CommentOperationLogs
{
    [LoggerMessage(EventId = 4020, Level = LogLevel.Information, Message = "Persisting comment {CommentId} for course {CourseId}")]
    public static partial void PersistingComment(this ILogger logger, string commentId, string courseId);

    [LoggerMessage(EventId = 4021, Level = LogLevel.Information, Message = "Updating comment {CommentId}")]
    public static partial void UpdatingComment(this ILogger logger, string commentId);

    [LoggerMessage(EventId = 4022, Level = LogLevel.Information, Message = "Deleting comment {CommentId}")]
    public static partial void DeletingComment(this ILogger logger, string commentId);

    [LoggerMessage(EventId = 4023, Level = LogLevel.Warning, Message = "Comment {CommentId} was not found for deletion")]
    public static partial void CommentNotFoundForDeletion(this ILogger logger, string commentId);

    [LoggerMessage(EventId = 4024, Level = LogLevel.Information, Message = "Comment {CommentId} created")]
    public static partial void CommentCreatedEvent(this ILogger logger, string commentId);

    [LoggerMessage(EventId = 4025, Level = LogLevel.Information, Message = "Comment {CommentId} updated")]
    public static partial void CommentUpdatedEvent(this ILogger logger, string commentId);

    [LoggerMessage(EventId = 4026, Level = LogLevel.Information, Message = "Comment {CommentId} deleted")]
    public static partial void CommentDeletedEvent(this ILogger logger, string commentId);
}
