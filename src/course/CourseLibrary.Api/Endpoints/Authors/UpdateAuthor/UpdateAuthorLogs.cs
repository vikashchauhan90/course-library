namespace CourseLibrary.Api.Endpoints.Authors.UpdateAuthor;

public static partial class UpdateAuthorLogs
{
    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Information,
        Message = "Updating author {AuthorId}")]
    public static partial void UpdatingAuthor(
        this ILogger logger,
        string authorId);

    [LoggerMessage(
        EventId = 1012,
        Level = LogLevel.Information,
        Message = "Author {AuthorId} updated")]
    public static partial void AuthorUpdated(
        this ILogger logger,
        string authorId);
}
