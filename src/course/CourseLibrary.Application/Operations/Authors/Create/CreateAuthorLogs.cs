using CourseLibrary.Domain;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Authors.Create;

internal static partial class CreateAuthorLogs
{
    [LoggerMessage(
       EventId = EventIds.Authors.CreateAuthor + 5,
       Level = LogLevel.Information,
       Message = "Author created: {AuthorId} {Name}")]
    public static partial void AuthorCreated(
       this ILogger logger,
       string authorId,
       string name);

    [LoggerMessage(
       EventId = EventIds.Authors.CreateAuthor + 4,
       Level = LogLevel.Information,
       Message = "Creating author {AuthorId} ({Name})")]
    public static partial void CreatingAuthor(
       this ILogger logger,
       string authorId,
       string name);
}
