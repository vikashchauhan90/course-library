using CourseLibrary.Domain;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Authors;

internal static partial class AuthorOperationLogs
{
    [LoggerMessage(EventId = EventIds.Authors.CreateAuthor + 20, Level = LogLevel.Information, Message = "Persisting author {AuthorId}")]
    public static partial void PersistingAuthor(this ILogger logger, string authorId);

    [LoggerMessage(EventId = EventIds.Authors.UpdateAuthor + 20, Level = LogLevel.Information, Message = "Updating author {AuthorId}")]
    public static partial void UpdatingAuthor(this ILogger logger, string authorId);

    [LoggerMessage(EventId = EventIds.Authors.DeleteAuthor + 20, Level = LogLevel.Information, Message = "Deleting author {AuthorId}")]
    public static partial void DeletingAuthor(this ILogger logger, string authorId);

    [LoggerMessage(EventId = EventIds.Authors.DeleteAuthor + 21, Level = LogLevel.Warning, Message = "Author {AuthorId} was not found for deletion")]
    public static partial void AuthorNotFoundForDeletion(this ILogger logger, string authorId);

    [LoggerMessage(EventId = EventIds.Authors.CreateAuthor + 22, Level = LogLevel.Information, Message = "Author {AuthorId} created")]
    public static partial void AuthorCreatedEvent(this ILogger logger, string authorId);

    [LoggerMessage(EventId = EventIds.Authors.UpdateAuthor + 22, Level = LogLevel.Information, Message = "Author {AuthorId} updated")]
    public static partial void AuthorUpdatedEvent(this ILogger logger, string authorId);

    [LoggerMessage(EventId = EventIds.Authors.DeleteAuthor + 22, Level = LogLevel.Information, Message = "Author {AuthorId} deleted")]
    public static partial void AuthorDeletedEvent(this ILogger logger, string authorId);
}
