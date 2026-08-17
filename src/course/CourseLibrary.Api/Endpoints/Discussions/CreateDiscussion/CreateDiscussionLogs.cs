namespace CourseLibrary.Api.Endpoints.Discussions.CreateDiscussion;

public static partial class CreateDiscussionLogs
{
    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Information,
        Message = "Creating discussion {Title}")]
    public static partial void CreatingDiscussion(
        this ILogger logger,
        string title);

    [LoggerMessage(
        EventId = 3005,
        Level = LogLevel.Information,
        Message = "Discussion {DiscussionId} created")]
    public static partial void DiscussionCreated(
        this ILogger logger,
        string discussionId);
}
