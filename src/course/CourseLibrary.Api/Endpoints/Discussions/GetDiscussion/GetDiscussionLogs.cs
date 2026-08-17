namespace CourseLibrary.Api.Endpoints.Discussions.GetDiscussion;

public static partial class GetDiscussionLogs
{
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Information,
        Message = "Getting discussion {DiscussionId}")]
    public static partial void GettingDiscussion(
        this ILogger logger,
        string discussionId);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Information,
        Message = "Discussion {DiscussionId} retrieved")]
    public static partial void DiscussionRetrieved(
        this ILogger logger,
        string discussionId);

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Warning,
        Message = "Discussion {DiscussionId} not found")]
    public static partial void DiscussionNotFound(
        this ILogger logger,
        string discussionId);
}
