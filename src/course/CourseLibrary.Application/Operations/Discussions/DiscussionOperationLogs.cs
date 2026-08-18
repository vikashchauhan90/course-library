using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Discussions;

internal static partial class DiscussionOperationLogs
{
    [LoggerMessage(EventId = 3020, Level = LogLevel.Information, Message = "Persisting discussion {DiscussionId} for course {CourseId}")]
    public static partial void PersistingDiscussion(this ILogger logger, string discussionId, string courseId);

    [LoggerMessage(EventId = 3021, Level = LogLevel.Information, Message = "Updating discussion {DiscussionId}")]
    public static partial void UpdatingDiscussion(this ILogger logger, string discussionId);

    [LoggerMessage(EventId = 3022, Level = LogLevel.Information, Message = "Deleting discussion {DiscussionId}")]
    public static partial void DeletingDiscussion(this ILogger logger, string discussionId);

    [LoggerMessage(EventId = 3023, Level = LogLevel.Warning, Message = "Discussion {DiscussionId} was not found for deletion")]
    public static partial void DiscussionNotFoundForDeletion(this ILogger logger, string discussionId);

    [LoggerMessage(EventId = 3024, Level = LogLevel.Information, Message = "Discussion {DiscussionId} created")]
    public static partial void DiscussionCreatedEvent(this ILogger logger, string discussionId);

    [LoggerMessage(EventId = 3025, Level = LogLevel.Information, Message = "Discussion {DiscussionId} updated")]
    public static partial void DiscussionUpdatedEvent(this ILogger logger, string discussionId);

    [LoggerMessage(EventId = 3026, Level = LogLevel.Information, Message = "Discussion {DiscussionId} deleted")]
    public static partial void DiscussionDeletedEvent(this ILogger logger, string discussionId);
}
