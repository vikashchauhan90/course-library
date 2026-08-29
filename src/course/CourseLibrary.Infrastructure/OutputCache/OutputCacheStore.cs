using CourseLibrary.Application.Abstractions.Caching;
using CourseLibrary.Infrastructure.Observability.Traces;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO.Hashing;
using System.Text;

namespace CourseLibrary.Infrastructure.Caching;

public sealed class OutputCacheStore(
    ICacheProvider cacheProvider,
    ILogger<OutputCacheStore> logger)
    : IOutputCacheStore
{
    public async ValueTask<byte[]?> GetAsync(
        string key,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var cacheKey = HashCacheKey(key);
        using var activity = ActivitySources.Infrastructure.StartActivity(
            "OutputCacheStore.GetAsync",
            ActivityKind.Internal);

        activity?.SetTag("cache.key", cacheKey);
        activity?.SetTag("cache.operation", "get");

        try
        {
            var value = await cacheProvider.GetOrCreateAsync(
                cacheKey,
                factory: _ => Task.FromResult<byte[]>(Array.Empty<byte>()),
                TimeSpan.FromSeconds(1), // Temporary TTL for the factory, won't be used since we return null
                null,
                cancellationToken);

            activity?.SetTag("cache.hit", value?.Length > 0);
            activity?.SetTag("cache.operation.success", true);

            logger.LogDebug(
                "Output cache {CacheResult} for key {CacheKey}",
                value is null ? "miss" : "hit",
                cacheKey);

            return value?.Length > 0 ? value : null;
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("cache.operation.cancelled", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

            logger.LogDebug(
                "Output cache get operation was cancelled for key {CacheKey}",
                cacheKey);

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag("cache.operation.error", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                ex.Message);

            logger.LogError(
                ex,
                "Error getting output cache entry for key {CacheKey}",
                cacheKey);

            throw;
        }
    }

    public async ValueTask SetAsync(
        string key,
        byte[] value,
        string[]? tags,
        TimeSpan validFor,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);
        var cacheKey = HashCacheKey(key);

        if (validFor <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(validFor),
                validFor,
                "Output cache expiration must be greater than zero.");
        }

        using var activity = ActivitySources.Infrastructure.StartActivity(
            "OutputCacheStore.SetAsync",
            ActivityKind.Internal);

        activity?.SetTag("cache.key", cacheKey);
        activity?.SetTag("cache.ttl", validFor.ToString());
        activity?.SetTag("cache.operation", "set");
        activity?.SetTag("cache.tag.count", tags?.Length ?? 0);

        try
        {
            await cacheProvider.SetAsync(
                cacheKey,
                value,
                validFor,
                tags,
                cancellationToken);

            activity?.SetTag("cache.operation.success", true);

            logger.LogDebug(
                "Output cache entry set for key {CacheKey} with TTL {CacheTtl}",
                cacheKey,
                validFor);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("cache.operation.cancelled", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

            logger.LogDebug(
                "Output cache set operation was cancelled for key {CacheKey}",
                cacheKey);

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag("cache.operation.error", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                ex.Message);

            logger.LogError(
                ex,
                "Error setting output cache entry for key {CacheKey}",
                cacheKey);

            throw;
        }
    }

    public async ValueTask EvictByTagAsync(
        string tag,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        using var activity = ActivitySources.Infrastructure.StartActivity(
            "OutputCacheStore.EvictByTagAsync",
            ActivityKind.Internal);

        activity?.SetTag("cache.tag", tag);
        activity?.SetTag("cache.operation", "evict-by-tag");

        try
        {
            await cacheProvider.RemoveByTagAsync(
                tag,
                cancellationToken);

            activity?.SetTag("cache.operation.success", true);

            logger.LogDebug(
                "Output cache entries evicted by tag {CacheTag}",
                tag);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("cache.operation.cancelled", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

            logger.LogDebug(
                "Output cache tag eviction was cancelled for tag {CacheTag}",
                tag);

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag("cache.operation.error", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                ex.Message);

            logger.LogError(
                ex,
                "Error evicting output cache entries for tag {CacheTag}",
                tag);

            throw;
        }
    }

    private static string HashCacheKey(string key)
    {
        var hash = XxHash128.Hash(Encoding.UTF8.GetBytes(key));

        return Convert.ToHexString(hash);
    }
}