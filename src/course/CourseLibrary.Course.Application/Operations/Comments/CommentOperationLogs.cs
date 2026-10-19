using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Application.Operations.Comments;

internal static partial class CommentOperationLogs
{
    [LoggerMessage(7210, LogLevel.Information, "Persisting comment {CommentId} for course {CourseId}.", EventName = nameof(PersistingComment))]
    public static partial void PersistingComment(this ILogger logger, string commentId, string courseId);

    [LoggerMessage(7211, LogLevel.Information, "Updating comment {CommentId}.", EventName = nameof(UpdatingComment))]
    public static partial void UpdatingComment(this ILogger logger, string commentId);

    [LoggerMessage(7212, LogLevel.Information, "Deleting comment {CommentId}.", EventName = nameof(DeletingComment))]
    public static partial void DeletingComment(this ILogger logger, string commentId);

    [LoggerMessage(7213, LogLevel.Warning, "Comment {CommentId} was not found for deletion.", EventName = nameof(CommentNotFoundForDeletion))]
    public static partial void CommentNotFoundForDeletion(this ILogger logger, string commentId);
}
