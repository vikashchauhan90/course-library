using CourseLibrary.Application.Abstractions.Caching;
using CourseLibrary.Infrastructure.Observability.Traces;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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

        using var activity = ActivitySources.Infrastructure.StartActivity(
            "OutputCacheStore.GetAsync",
            ActivityKind.Internal);

        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.operation", "get");

        try
        {
            var value = await cacheProvider.GetOrCreateAsync(
                key,
                factory: _ => Task.FromResult<byte[]?>(null)!,
                TimeSpan.FromSeconds(1), // Temporary TTL for the factory, won't be used since we return null
                null,
                cancellationToken);

            activity?.SetTag("cache.hit", value?.Length > 0);
            activity?.SetTag("cache.operation.success", true);

            logger.LogDebug(
                "Output cache {CacheResult} for key {CacheKey}",
                value is null ? "miss" : "hit",
                key);

            return value;
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("cache.operation.cancelled", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

            logger.LogDebug(
                "Output cache get operation was cancelled for key {CacheKey}",
                key);

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
                key);

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

        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.ttl", validFor.ToString());
        activity?.SetTag("cache.operation", "set");
        activity?.SetTag("cache.tag.count", tags?.Length ?? 0);

        try
        {
            await cacheProvider.SetAsync(
                key,
                value,
                validFor,
                tags,
                cancellationToken);

            activity?.SetTag("cache.operation.success", true);

            logger.LogDebug(
                "Output cache entry set for key {CacheKey} with TTL {CacheTtl}",
                key,
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
                key);

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
                key);

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
}