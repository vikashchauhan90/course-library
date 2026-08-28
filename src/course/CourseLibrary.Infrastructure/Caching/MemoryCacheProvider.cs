using CourseLibrary.Application.Abstractions.Caching;
using CourseLibrary.Infrastructure.Observability.Traces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CourseLibrary.Infrastructure.Caching;

public sealed class MemoryCacheProvider(
    IMemoryCache cache,
    ILogger<MemoryCacheProvider> logger)
    : ICacheProvider
{
    private readonly ConcurrentDictionary<string, AsyncLock> _locks = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<string>> _tagIndex = new();

    public async Task<byte[]> GetOrCreateAsync(
        string key,
        Func<CancellationToken, Task<byte[]>> factory,
        TimeSpan ttl,
        IEnumerable<string>? tags = null,
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

            IndexTags(key, tags);

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
        IEnumerable<string>? tags = null,
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

        IndexTags(key, tags);

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
        RemoveKeyFromTagIndex(key);

        logger.LogDebug(
            "Memory cache entry removed for key {CacheKey}",
            key);

        return Task.CompletedTask;
    }

    public Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        using var activity = ActivitySources.Infrastructure.StartActivity("MemoryCacheProvider.RemoveByTagAsync", System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag("cache.tag", tag);
        activity?.SetTag("cache.operation", "remove-by-tag");
        cancellationToken.ThrowIfCancellationRequested();

        if (_tagIndex.TryRemove(tag, out var keys))
        {
            foreach (var key in keys)
            {
                cache.Remove(key);
                logger.LogDebug(
                    "Memory cache entry removed for key {CacheKey} by tag {CacheTag}",
                    key,
                    tag);
            }
        }

        return Task.CompletedTask;
    }

    private void IndexTags(string key, IEnumerable<string>? tags)
    {
        // Remove old tag associations first
        RemoveKeyFromTagIndex(key);

        if (tags == null) return;

        foreach (var tag in tags)
        {
            if (string.IsNullOrWhiteSpace(tag)) continue;

            var keys = _tagIndex.GetOrAdd(tag, _ => new ConcurrentBag<string>());
            keys.Add(key);
        }
    }

    private void RemoveKeyFromTagIndex(string key)
    {
        foreach (var tagPair in _tagIndex)
        {
            var tag = tagPair.Key;
            var keys = tagPair.Value;

            // Create a new bag without the key
            var remainingKeys = new ConcurrentBag<string>(
                keys.Where(k => k != key));

            if (remainingKeys.IsEmpty)
            {
                _tagIndex.TryRemove(tag, out _);
            }
            else
            {
                _tagIndex[tag] = remainingKeys;
            }
        }
    }
}