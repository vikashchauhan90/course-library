namespace CourseLibrary.Api.Endpoints.Authors.GetAuthors;

public static partial class GetAuthorsLogs
{
    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Information,
        Message = "Getting all authors")]
    public static partial void GettingAllAuthors(
        this ILogger logger);

    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Information,
        Message = "Retrieved {AuthorCount} authors")]
    public static partial void AuthorsRetrieved(
        this ILogger logger,
        int authorCount);
}
