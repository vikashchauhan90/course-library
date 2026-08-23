using CourseLibrary.Application.Abstractions.Caching;
using CourseLibrary.Infrastructure.Observability.Traces;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Infrastructure.Caching;

public sealed class HybridCacheProvider(
    HybridCache cache,
    ILogger<HybridCacheProvider> logger) : ICacheProvider
{
    public async Task<byte[]> GetOrCreateAsync(
        string key,
        Func<CancellationToken, Task<byte[]>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        using var activity = ActivitySources.Infrastructure.StartActivity(
            "HybridCacheProvider.GetOrCreateAsync",
            ActivityKind.Internal);
        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.ttl", ttl.ToString());
        activity?.SetTag("cache.operation", "get-or-create");

        try
        {
            var options = new HybridCacheEntryOptions
            {
                Expiration = ttl
            };

            var value = await cache.GetOrCreateAsync(
                key,
                async ct =>
                {
                    logger.LogDebug(
                        "Cache miss for key {CacheKey}. Executing factory.",
                        key);

                    return await factory(ct);
                },
                options,
                cancellationToken: cancellationToken);

            logger.LogDebug(
                "Cache get-or-create completed for key {CacheKey}",
                key);

            return value;
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("error", true)
                .SetTag("error.message", "Cache get-or-create operation was cancelled.");
            logger.LogDebug(
                "Cache get-or-create operation was cancelled for key {CacheKey}",
                key);

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", true)
                  .SetTag("error.message", ex.Message)
                  .SetTag("error.stacktrace", ex.StackTrace);
            logger.LogError(
                ex,
                "Error getting or creating cache entry for key {CacheKey}",
                key);

            throw;
        }
    }

    public async Task SetAsync(
        string key,
        byte[] value,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        using var activity = ActivitySources.Infrastructure.StartActivity(
            "HybridCacheProvider.SetAsync",
            ActivityKind.Internal);
        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.ttl", ttl.ToString());
        activity?.SetTag("cache.operation", "set"); 
        if (ttl <= TimeSpan.Zero)
        {
            activity?.SetTag("error", true)
                .SetTag("error.message", "Cache expiration must be greater than zero.");
            throw new ArgumentOutOfRangeException(
                nameof(ttl),
                ttl,
                "Cache expiration must be greater than zero.");
        }

        try
        {
            var options = new HybridCacheEntryOptions
            {
                Expiration = ttl
            };

            await cache.SetAsync(
                key,
                value,
                options,
                cancellationToken: cancellationToken);

            logger.LogDebug(
                "Cache entry set for key {CacheKey} with TTL {CacheTtl}",
                key,
                ttl);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("error", true)
                .SetTag("error.message", "Cache set operation was cancelled.");
            logger.LogDebug(
                "Cache set operation was cancelled for key {CacheKey}",
                key);

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", true)
                .SetTag("error.message", ex.Message)
                .SetTag("error.stacktrace", ex.StackTrace);
            logger.LogError(
                ex,
                "Error setting cache entry for key {CacheKey}",
                key);

            throw;
        }
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        using var activity = ActivitySources.Infrastructure.StartActivity(
            "HybridCacheProvider.RemoveAsync",
            ActivityKind.Internal);

        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.operation", "remove");
        try
        {
            await cache.RemoveAsync(
                key,
                cancellationToken);

            logger.LogDebug(
                "Cache entry removed for key {CacheKey}",
                key);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("error", true)
              .SetTag("error.message", "Cache set operation was cancelled.");

            logger.LogDebug(
                "Cache remove operation was cancelled for key {CacheKey}",
                key);

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", true)
                .SetTag("error.message", ex.Message)
                .SetTag("error.stacktrace", ex.StackTrace);
            logger.LogError(
                ex,
                "Error removing cache entry for key {CacheKey}",
                key);

            throw;
        }
    }
}