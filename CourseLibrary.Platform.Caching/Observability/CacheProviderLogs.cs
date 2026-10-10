using Microsoft.Extensions.Logging;

namespace CourseLibrary.Platform.Caching.Observability;

internal static partial class CacheProviderLogs
{
    [LoggerMessage(5001, LogLevel.Debug, "Cache lookup {CacheResult}.")]
    public static partial void LookupResult(this ILogger logger, string cacheResult);

    [LoggerMessage(5002, LogLevel.Debug, "Cache miss. Executing factory.")]
    public static partial void MissExecutingFactory(this ILogger logger);

    [LoggerMessage(5003, LogLevel.Debug, "Cache get-or-create completed.")]
    public static partial void GetOrCreateCompleted(this ILogger logger);

    [LoggerMessage(5004, LogLevel.Debug, "Cache entry set.")]
    public static partial void EntrySet(this ILogger logger);

    [LoggerMessage(5005, LogLevel.Debug, "Cache entry removed.")]
    public static partial void EntryRemoved(this ILogger logger);

    [LoggerMessage(5006, LogLevel.Debug, "Cache entries invalidated by tag.")]
    public static partial void EntriesRemovedByTag(this ILogger logger);

    [LoggerMessage(5007, LogLevel.Debug, "Cache try-add {CacheResult}.")]
    public static partial void TryAddResult(this ILogger logger, string cacheResult);

    [LoggerMessage(5008, LogLevel.Debug, "Cache try-add operation was cancelled.")]
    public static partial void TryAddCancelled(this ILogger logger);

    [LoggerMessage(5009, LogLevel.Error, "Cache operation {CacheOperation} failed.")]
    public static partial void OperationFailed(
        this ILogger logger,
        Exception exception,
        string cacheOperation);
}