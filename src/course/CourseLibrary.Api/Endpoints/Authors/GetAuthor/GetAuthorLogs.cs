namespace CourseLibrary.Api.Endpoints.Authors.GetAuthor;

public static partial class GetAuthorLogs
{
    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Information,
        Message = "Getting author {AuthorId}")]
    public static partial void GettingAuthor(
        this ILogger logger,
        string authorId);

    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Information,
        Message = "Author {AuthorId} retrieved")]
    public static partial void AuthorRetrieved(
        this ILogger logger,
        string authorId);

    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Warning,
        Message = "Author {AuthorId} not found")]
    public static partial void AuthorNotFound(
        this ILogger logger,
        string authorId);
}
