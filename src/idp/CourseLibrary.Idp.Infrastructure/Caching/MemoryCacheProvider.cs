using System.Collections.Concurrent;
using CourseLibrary.Idp.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Idp.Infrastructure.Caching;

public sealed class MemoryCacheProvider(
    IMemoryCache cache,
    ILogger<MemoryCacheProvider> logger) : ICacheProvider
{
    private readonly ConcurrentDictionary<string, AsyncLock> _locks = new();

    public async Task<byte[]> GetOrCreateAsync(
        string key,
        Func<CancellationToken, Task<byte[]>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        // Fast path.
        if (cache.TryGetValue(key, out byte[]? cachedValue) &&
            cachedValue is not null)
        {
            logger.LogDebug(
                "Memory cache hit for key {CacheKey}",
                key);

            return cachedValue;
        }

        var asyncLock = _locks.GetOrAdd(
            key,
            static _ => new AsyncLock());

        using (await asyncLock.LockAsync(cancellationToken))
        {
            // Double-check after acquiring the lock.
            if (cache.TryGetValue(key, out cachedValue) &&
                cachedValue is not null)
            {
                logger.LogDebug(
                    "Memory cache hit after waiting for key {CacheKey}",
                    key);

                return cachedValue;
            }

            logger.LogDebug(
                "Memory cache miss for key {CacheKey}. Executing factory.",
                key);

            var value = await factory(cancellationToken);

            ArgumentNullException.ThrowIfNull(value);

            cache.Set(
                key,
                value,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                });

            logger.LogDebug(
                "Memory cache entry created for key {CacheKey} with TTL {CacheTtl}",
                key,
                ttl);

            return value;
        }
    }

    public Task SetAsync(
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

        cancellationToken.ThrowIfCancellationRequested();

        cache.Set(
            key,
            value,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            });

        logger.LogDebug(
            "Memory cache entry set for key {CacheKey} with TTL {CacheTtl}",
            key,
            ttl);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        cancellationToken.ThrowIfCancellationRequested();

        cache.Remove(key);

        logger.LogDebug(
            "Memory cache entry removed for key {CacheKey}",
            key);

        return Task.CompletedTask;
    }
}
