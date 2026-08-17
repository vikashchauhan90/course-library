using CourseLibrary.Domain;

namespace CourseLibrary.Api.Endpoints.Authors.CreateAuthor;

public static partial class CreateAuthorLogs
{
    [LoggerMessage(
        EventId = EventIds.Authors.CreateAuthor + 1,
        Level = LogLevel.Information,
        Message = "Creating author {AuthorName}")]
    public static partial void CreatingAuthor(
        this ILogger logger,
        string authorName);

    [LoggerMessage(
    EventId = EventIds.Authors.CreateAuthor + 2,
    Level = LogLevel.Information,
    Message = "Author {AuthorName} created with ID {AuthorId}")]
    public static partial void AuthorCreated(
        this ILogger logger,
        string authorName,
        string authorId);
}