using CourseLibrary.Idp.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Idp.Infrastructure.Caching;

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

        try
        {
            var cachedValue = await cache.GetAsync(
                key,
                cancellationToken);

            if (cachedValue is not null)
            {
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
            logger.LogDebug(
                "Redis cache operation was cancelled for key {CacheKey}",
                key);

            throw;
        }
        catch (Exception ex)
        {
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

        if (ttl <= TimeSpan.Zero)
        {
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

            logger.LogDebug(
                "Redis cache entry set for key {CacheKey} with TTL {CacheTtl}",
                key,
                ttl);
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug(
                "Redis cache set operation was cancelled for key {CacheKey}",
                key);

            throw;
        }
        catch (Exception ex)
        {
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

        try
        {
            await cache.RemoveAsync(
                key,
                cancellationToken);

            logger.LogDebug(
                "Redis cache entry removed for key {CacheKey}",
                key);
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug(
                "Redis cache remove operation was cancelled for key {CacheKey}",
                key);

            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error removing Redis cache entry for key {CacheKey}",
                key);

            throw;
        }
    }
}