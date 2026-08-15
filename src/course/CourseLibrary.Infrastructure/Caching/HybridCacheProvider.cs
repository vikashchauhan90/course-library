using CourseLibrary.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

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
            logger.LogDebug(
                "Cache get-or-create operation was cancelled for key {CacheKey}",
                key);

            throw;
        }
        catch (Exception ex)
        {
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

        if (ttl <= TimeSpan.Zero)
        {
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
            logger.LogDebug(
                "Cache set operation was cancelled for key {CacheKey}",
                key);

            throw;
        }
        catch (Exception ex)
        {
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
            logger.LogDebug(
                "Cache remove operation was cancelled for key {CacheKey}",
                key);

            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error removing cache entry for key {CacheKey}",
                key);

            throw;
        }
    }
}