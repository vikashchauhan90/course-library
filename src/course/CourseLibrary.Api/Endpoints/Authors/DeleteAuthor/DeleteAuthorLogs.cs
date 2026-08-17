namespace CourseLibrary.Api.Endpoints.Authors.DeleteAuthor;

public static partial class DeleteAuthorLogs
{
    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Information,
        Message = "Deleting author {AuthorId}")]
    public static partial void DeletingAuthor(
        this ILogger logger,
        string authorId);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Information,
        Message = "Author {AuthorId} deleted")]
    public static partial void AuthorDeleted(
        this ILogger logger,
        string authorId);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Warning,
        Message = "Author {AuthorId} not found for deletion")]
    public static partial void AuthorNotFoundForDeletion(
        this ILogger logger,
        string authorId);
}
