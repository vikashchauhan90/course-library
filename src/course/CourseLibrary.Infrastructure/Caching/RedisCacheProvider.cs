using CourseLibrary.Application.Abstractions.Caching;
using CourseLibrary.Infrastructure.Observability.Traces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Infrastructure.Caching;

public sealed class RedisCacheProvider(
    IDistributedCache cache,
    ILogger<RedisCacheProvider> logger) : ICacheProvider
{
    public async Task<byte[]> GetOrCreateAsync(
        string key,
        Func<CancellationToken, Task<byte[]>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        using var activity = ActivitySources.Infrastructure.StartActivity("RedisCacheProvider.GetOrCreateAsync", ActivityKind.Internal);
        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.ttl", ttl.ToString());
        activity?.SetTag("cache.operation", "get-or-create");

        try
        {
            var cachedValue = await cache.GetAsync(
                key,
                cancellationToken);

            if (cachedValue is not null)
            {
                activity?.SetTag("cache.hit", true);
                logger.LogDebug(
                    "Redis cache hit for key {CacheKey}",
                    key);

                return cachedValue;
            }

            logger.LogDebug(
                "Redis cache miss for key {CacheKey}. Executing factory.",
                key);

            var value = await factory(cancellationToken);

            ArgumentNullException.ThrowIfNull(value);

            activity?.SetTag("cache.hit", false);
            await cache.SetAsync(
                key,
                value,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                },
                cancellationToken);

            logger.LogDebug(
                "Redis cache entry created for key {CacheKey} with TTL {CacheTtl}",
                key,
                ttl);

            return value;
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("cache.operation.cancelled", true);
            activity?.SetStatus(ActivityStatusCode.Error, "Operation was cancelled.");
            logger.LogDebug(
                "Redis cache operation was cancelled for key {CacheKey}",
                key);

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag("cache.operation.error", true);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("cache.operation.exception", ex.ToString());
            logger.LogError(
                ex,
                "Error getting or creating Redis cache entry for key {CacheKey}",
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

        using var activity = ActivitySources.Infrastructure.StartActivity("RedisCacheProvider.SetAsync", ActivityKind.Internal);
        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.ttl", ttl.ToString());
        activity?.SetTag("cache.operation", "set");

        if (ttl <= TimeSpan.Zero)
        {
            activity?.SetTag("cache.operation.error", true);
            activity?.SetStatus(ActivityStatusCode.Error, "Cache expiration must be greater than zero.");
            throw new ArgumentOutOfRangeException(
                nameof(ttl),
                ttl,
                "Cache expiration must be greater than zero.");
        }

        try
        {
            await cache.SetAsync(
                key,
                value,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                },
                cancellationToken);

            activity?.SetTag("cache.operation.success", true);

            logger.LogDebug(
                "Redis cache entry set for key {CacheKey} with TTL {CacheTtl}",
                key,
                ttl);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("cache.operation.cancelled", true);
            activity?.SetStatus(ActivityStatusCode.Error, "Operation was cancelled.");
            logger.LogDebug(
                "Redis cache set operation was cancelled for key {CacheKey}",
                key);

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag("cache.operation.error", true);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("cache.operation.exception", ex.ToString());
            logger.LogError(
                ex,
                "Error setting Redis cache entry for key {CacheKey}",
                key);

            throw;
        }
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        using var activity = ActivitySources.Infrastructure.StartActivity("RedisCacheProvider.RemoveAsync", ActivityKind.Internal);
        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.operation", "remove");
        activity?.SetTag("cache.operation.success", false);
        try
        {
            await cache.RemoveAsync(
                key,
                cancellationToken);

            activity?.SetTag("cache.operation.success", true);
            logger.LogDebug(
                "Redis cache entry removed for key {CacheKey}",
                key);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("cache.operation.cancelled", true);
            activity?.SetStatus(ActivityStatusCode.Error, "Operation was cancelled.");
            logger.LogDebug(
                "Redis cache remove operation was cancelled for key {CacheKey}",
                key);

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag("cache.operation.error", true);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("cache.operation.exception", ex.ToString());
            logger.LogError(
                ex,
                "Error removing Redis cache entry for key {CacheKey}",
                key);

            throw;
        }
    }
}