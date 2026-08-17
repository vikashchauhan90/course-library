namespace CourseLibrary.Api.Endpoints.Discussions.UpdateDiscussion;

public static partial class UpdateDiscussionLogs
{
    [LoggerMessage(
        EventId = 3006,
        Level = LogLevel.Information,
        Message = "Updating discussion {DiscussionId}")]
    public static partial void UpdatingDiscussion(
        this ILogger logger,
        string discussionId);

    [LoggerMessage(
        EventId = 3007,
        Level = LogLevel.Information,
        Message = "Discussion {DiscussionId} updated")]
    public static partial void DiscussionUpdated(
        this ILogger logger,
        string discussionId);
}
