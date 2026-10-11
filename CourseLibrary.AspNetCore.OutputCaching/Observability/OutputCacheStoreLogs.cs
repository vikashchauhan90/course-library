using Microsoft.Extensions.Logging;

namespace CourseLibrary.AspNetCore.OutputCaching.Observability;

internal static partial class OutputCacheStoreLogs
{
    [LoggerMessage(5101, LogLevel.Debug, "Output cache {CacheResult} for key {CacheKey}.")]
    public static partial void GetResult(
        this ILogger logger,
        string cacheResult,
        string cacheKey);

    [LoggerMessage(5102, LogLevel.Debug, "Output cache get operation was cancelled for key {CacheKey}.")]
    public static partial void GetCancelled(this ILogger logger, string cacheKey);

    [LoggerMessage(5103, LogLevel.Error, "Error getting output cache entry for key {CacheKey}.")]
    public static partial void GetFailed(
        this ILogger logger,
        Exception exception,
        string cacheKey);

    [LoggerMessage(5104, LogLevel.Debug, "Output cache entry set for key {CacheKey} with TTL {CacheTtl}.")]
    public static partial void EntrySet(
        this ILogger logger,
        string cacheKey,
        TimeSpan cacheTtl);

    [LoggerMessage(5105, LogLevel.Debug, "Output cache set operation was cancelled for key {CacheKey}.")]
    public static partial void SetCancelled(this ILogger logger, string cacheKey);

    [LoggerMessage(5106, LogLevel.Error, "Error setting output cache entry for key {CacheKey}.")]
    public static partial void SetFailed(
        this ILogger logger,
        Exception exception,
        string cacheKey);

    [LoggerMessage(5107, LogLevel.Debug, "Output cache entries evicted by tag {CacheTag}.")]
    public static partial void EntriesEvictedByTag(this ILogger logger, string cacheTag);

    [LoggerMessage(5108, LogLevel.Debug, "Output cache tag eviction was cancelled for tag {CacheTag}.")]
    public static partial void TagEvictionCancelled(this ILogger logger, string cacheTag);

    [LoggerMessage(5109, LogLevel.Error, "Error evicting output cache entries for tag {CacheTag}.")]
    public static partial void TagEvictionFailed(
        this ILogger logger,
        Exception exception,
        string cacheTag);
}