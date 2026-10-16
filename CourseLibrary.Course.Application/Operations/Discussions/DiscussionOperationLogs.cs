using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Application.Operations.Discussions;

internal static partial class DiscussionOperationLogs
{
    [LoggerMessage(7310, LogLevel.Information, "Persisting discussion {DiscussionId} for course {CourseId}.", EventName = nameof(PersistingDiscussion))]
    public static partial void PersistingDiscussion(this ILogger logger, string discussionId, string courseId);

    [LoggerMessage(7311, LogLevel.Information, "Updating discussion {DiscussionId}.", EventName = nameof(UpdatingDiscussion))]
    public static partial void UpdatingDiscussion(this ILogger logger, string discussionId);

    [LoggerMessage(7312, LogLevel.Information, "Deleting discussion {DiscussionId}.", EventName = nameof(DeletingDiscussion))]
    public static partial void DeletingDiscussion(this ILogger logger, string discussionId);

    [LoggerMessage(7313, LogLevel.Warning, "Discussion {DiscussionId} was not found for deletion.", EventName = nameof(DiscussionNotFoundForDeletion))]
    public static partial void DiscussionNotFoundForDeletion(this ILogger logger, string discussionId);
}
