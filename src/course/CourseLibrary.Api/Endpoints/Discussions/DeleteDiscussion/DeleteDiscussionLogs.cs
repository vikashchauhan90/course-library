namespace CourseLibrary.Api.Endpoints.Discussions.DeleteDiscussion;

public static partial class DeleteDiscussionLogs
{
    [LoggerMessage(
        EventId = 3008,
        Level = LogLevel.Information,
        Message = "Deleting discussion {DiscussionId}")]
    public static partial void DeletingDiscussion(
        this ILogger logger,
        string discussionId);

    [LoggerMessage(
        EventId = 3009,
        Level = LogLevel.Information,
        Message = "Discussion {DiscussionId} deleted")]
    public static partial void DiscussionDeleted(
        this ILogger logger,
        string discussionId);

    [LoggerMessage(
        EventId = 3010,
        Level = LogLevel.Warning,
        Message = "Discussion {DiscussionId} not found for deletion")]
    public static partial void DiscussionNotFoundForDeletion(
        this ILogger logger,
        string discussionId);
}
