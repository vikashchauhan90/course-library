using CourseLibrary.Application.Abstractions.Caching;
using CourseLibrary.Infrastructure.Observability.Traces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CourseLibrary.Infrastructure.Caching;

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

        using var activity = ActivitySources.Infrastructure.StartActivity("MemoryCacheProvider.GetOrCreateAsync");
        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.ttl", ttl.ToString());
        activity?.SetTag("cache.operation", "get-or-create");

        // Fast path.
        if (cache.TryGetValue(key, out byte[]? cachedValue) &&
            cachedValue is not null)
        {
            activity?.SetTag("cache.hit", true);
            logger.LogDebug(
                "Memory cache hit for key {CacheKey}",
                key);

            return cachedValue;
        }

        activity?.SetTag("cache.hit", false);
        var asyncLock = _locks.GetOrAdd(
            key,
            static _ => new AsyncLock());

        using (await asyncLock.LockAsync(cancellationToken))
        {
            // Double-check after acquiring the lock.
            if (cache.TryGetValue(key, out cachedValue) &&
                cachedValue is not null)
            {
                activity?.SetTag("cache.hit", true);
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

            activity?.SetTag("cache.created", true);
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

        using var activity = ActivitySources.Infrastructure.StartActivity("MemoryCacheProvider.SetAsync", System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.ttl", ttl.ToString());
        activity?.SetTag("cache.operation", "set");
        if (ttl <= TimeSpan.Zero)
        {
            activity?.SetTag("cache.error", true);
            activity?.SetTag("cache.error.message", "Cache expiration must be greater than zero.");
            throw new ArgumentOutOfRangeException(
                nameof(ttl),
                ttl,
                "Cache expiration must be greater than zero.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        activity?.SetTag("cache.created", true);
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

        using var activity = ActivitySources.Infrastructure.StartActivity("MemoryCacheProvider.RemoveAsync", System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.operation", "remove");
        cancellationToken.ThrowIfCancellationRequested();

        cache.Remove(key);

        logger.LogDebug(
            "Memory cache entry removed for key {CacheKey}",
            key);

        return Task.CompletedTask;
    }
}
