using CourseLibrary.Application.Abstractions.Caching;
using CourseLibrary.Infrastructure.Observability.Traces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace CourseLibrary.Infrastructure.Caching;

public sealed class RedisCacheProvider(
    IDistributedCache cache,
    ILogger<RedisCacheProvider> logger)
    : ICacheProvider
{
    private const string TagKeyPrefix = "__tag:";
    private const string KeyTagsPrefix = "__keytags:";

    public async Task<byte[]> GetOrCreateAsync(
        string key,
        Func<CancellationToken, Task<byte[]>> factory,
        TimeSpan ttl,
        IEnumerable<string>? tags = null,
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

            // Set the cache entry
            await cache.SetAsync(
                key,
                value,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                },
                cancellationToken);

            // Index the tags
            await IndexTagsAsync(key, tags, ttl, cancellationToken);

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
        IEnumerable<string>? tags = null,
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
            // Remove old tag associations first
            await RemoveKeyFromAllTagIndexesAsync(key, cancellationToken);

            // Set the cache entry
            await cache.SetAsync(
                key,
                value,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                },
                cancellationToken);

            // Index the new tags
            await IndexTagsAsync(key, tags, ttl, cancellationToken);

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
            // Remove the cache entry
            await cache.RemoveAsync(
                key,
                cancellationToken);

            // Clean up tag indexes
            await RemoveKeyFromAllTagIndexesAsync(key, cancellationToken);

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

    public async Task RemoveByTagAsync(
        string tag,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        using var activity = ActivitySources.Infrastructure.StartActivity("RedisCacheProvider.RemoveByTagAsync", ActivityKind.Internal);
        activity?.SetTag("cache.tag", tag);
        activity?.SetTag("cache.operation", "remove-by-tag");

        try
        {
            var tagKey = GetTagKey(tag);
            var keysData = await cache.GetAsync(tagKey, cancellationToken);

            if (keysData is not null)
            {
                var keys = DeserializeKeys(keysData);

                foreach (var key in keys)
                {
                    // Remove the cache entry
                    await cache.RemoveAsync(key, cancellationToken);

                    // Clean up the reverse index
                    var keyTagsKey = GetKeyTagsKey(key);
                    await cache.RemoveAsync(keyTagsKey, cancellationToken);

                    logger.LogDebug(
                        "Redis cache entry removed for key {CacheKey} by tag {CacheTag}",
                        key,
                        tag);
                }

                // Remove the tag index itself
                await cache.RemoveAsync(tagKey, cancellationToken);
            }

            activity?.SetTag("cache.operation.success", true);
            activity?.SetTag("cache.removed_count", keysData is not null ? DeserializeKeys(keysData).Count() : 0);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("cache.operation.cancelled", true);
            activity?.SetStatus(ActivityStatusCode.Error, "Operation was cancelled.");
            logger.LogDebug(
                "Redis cache remove by tag operation was cancelled for tag {CacheTag}",
                tag);

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag("cache.operation.error", true);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("cache.operation.exception", ex.ToString());
            logger.LogError(
                ex,
                "Error removing Redis cache entries for tag {CacheTag}",
                tag);

            throw;
        }
    }

    private async Task IndexTagsAsync(
        string key,
        IEnumerable<string>? tags,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        if (tags == null) return;

        var tagList = tags.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
        if (tagList.Count == 0) return;

        // Store the reverse index (key -> tags)
        var keyTagsKey = GetKeyTagsKey(key);
        var serializedTagList = SerializeKeys(tagList);
        await cache.SetAsync(
            keyTagsKey,
            serializedTagList,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            },
            cancellationToken);

        // Update each tag index (tag -> keys)
        foreach (var tag in tagList)
        {
            var tagKey = GetTagKey(tag);

            // Get existing keys for this tag
            var existingKeysData = await cache.GetAsync(tagKey, cancellationToken);
            var keyList = existingKeysData is not null
                ? DeserializeKeys(existingKeysData).ToList()
                : new List<string>();

            // Add the new key if it doesn't exist
            if (!keyList.Contains(key))
            {
                keyList.Add(key);

                // Store the updated list
                var serializedKeys = SerializeKeys(keyList);
                await cache.SetAsync(
                    tagKey,
                    serializedKeys,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = ttl
                    },
                    cancellationToken);

                logger.LogDebug(
                    "Added key {CacheKey} to tag index {CacheTag}",
                    key,
                    tag);
            }
        }
    }

    private async Task RemoveKeyFromAllTagIndexesAsync(
        string key,
        CancellationToken cancellationToken)
    {
        // Get the tags associated with this key
        var keyTagsKey = GetKeyTagsKey(key);
        var tagsData = await cache.GetAsync(keyTagsKey, cancellationToken);

        if (tagsData is null) return;

        var tags = DeserializeKeys(tagsData);

        foreach (var tag in tags)
        {
            var tagKey = GetTagKey(tag);
            var keysData = await cache.GetAsync(tagKey, cancellationToken);

            if (keysData is null) continue;

            var keys = DeserializeKeys(keysData).ToList();

            if (keys.Remove(key))
            {
                if (keys.Count > 0)
                {
                    // Update the tag index with the remaining keys
                    var serializedKeys = SerializeKeys(keys);

                    // Get the original TTL to preserve it
                    // Since IDistributedCache doesn't expose TTL, we'll use a default
                    await cache.SetAsync(
                        tagKey,
                        serializedKeys,
                        new DistributedCacheEntryOptions
                        {
                            // Use a reasonable default TTL
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                        },
                        cancellationToken);
                }
                else
                {
                    // No more keys for this tag, remove the tag index
                    await cache.RemoveAsync(tagKey, cancellationToken);
                }

                logger.LogDebug(
                    "Removed key {CacheKey} from tag index {CacheTag}",
                    key,
                    tag);
            }
        }

        // Remove the reverse index
        await cache.RemoveAsync(keyTagsKey, cancellationToken);
    }

    private static string GetTagKey(string tag) => $"{TagKeyPrefix}{tag}";

    private static string GetKeyTagsKey(string key) => $"{KeyTagsPrefix}{key}";

    private static byte[] SerializeKeys(IEnumerable<string> keys)
    {
        var json = JsonSerializer.Serialize(keys);
        return Encoding.UTF8.GetBytes(json);
    }

    private static IEnumerable<string> DeserializeKeys(byte[] data)
    {
        var json = Encoding.UTF8.GetString(data);
        return JsonSerializer.Deserialize<IEnumerable<string>>(json) ?? Array.Empty<string>();
    }
}