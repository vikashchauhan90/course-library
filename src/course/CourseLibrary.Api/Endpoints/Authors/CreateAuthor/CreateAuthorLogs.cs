namespace CourseLibrary.Api.Endpoints.Authors.CreateAuthor;

public static partial class CreateAuthorLogs
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Creating author {AuthorId}")]
    public static partial void CreatingAuthor(
        this ILogger logger,
        string authorId);

    [LoggerMessage(
    EventId = 1002,
    Level = LogLevel.Information,
    Message = "Author {AuthorId} created")]
    public static partial void AuthorCreated(
        this ILogger logger,
        string authorId);
}