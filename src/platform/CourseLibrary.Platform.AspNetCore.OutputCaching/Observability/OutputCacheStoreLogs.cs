using Microsoft.Extensions.Logging;

namespace CourseLibrary.Platform.AspNetCore.OutputCaching.Observability;

internal static partial class OutputCacheStoreLogs
{
    [LoggerMessage(5101, LogLevel.Debug, "Output cache {CacheResult}.", EventName = nameof(GetResult))]
    public static partial void GetResult(
        this ILogger logger,
        string cacheResult);

    [LoggerMessage(5102, LogLevel.Debug, "Output cache get operation was cancelled.", EventName = nameof(GetCancelled))]
    public static partial void GetCancelled(this ILogger logger);

    [LoggerMessage(5103, LogLevel.Error, "Error getting output cache entry.", EventName = nameof(GetFailed))]
    public static partial void GetFailed(
        this ILogger logger,
        Exception exception);

    [LoggerMessage(5104, LogLevel.Debug, "Output cache entry set with TTL {CacheTtl}.", EventName = nameof(EntrySet))]
    public static partial void EntrySet(
        this ILogger logger,
        TimeSpan cacheTtl);

    [LoggerMessage(5105, LogLevel.Debug, "Output cache set operation was cancelled.", EventName = nameof(SetCancelled))]
    public static partial void SetCancelled(this ILogger logger);

    [LoggerMessage(5106, LogLevel.Error, "Error setting output cache entry.", EventName = nameof(SetFailed))]
    public static partial void SetFailed(
        this ILogger logger,
        Exception exception);

    [LoggerMessage(5107, LogLevel.Debug, "Output cache entries evicted by tag.", EventName = nameof(EntriesEvictedByTag))]
    public static partial void EntriesEvictedByTag(this ILogger logger);

    [LoggerMessage(5108, LogLevel.Debug, "Output cache tag eviction was cancelled.", EventName = nameof(TagEvictionCancelled))]
    public static partial void TagEvictionCancelled(this ILogger logger);

    [LoggerMessage(5109, LogLevel.Error, "Error evicting output cache entries by tag.", EventName = nameof(TagEvictionFailed))]
    public static partial void TagEvictionFailed(
        this ILogger logger,
        Exception exception);
}