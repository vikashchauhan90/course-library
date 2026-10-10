
using CourseLibrary.Platform.Caching.Abstractions;
using CourseLibrary.Platform.Caching.Observability;
using CourseLibrary.Platform.Observability.Metrics;
using CourseLibrary.Platform.Observability.Traces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Diagnostics;
using ZiggyCreatures.Caching.Fusion;

namespace CourseLibrary.Platform.Caching.Providers;

internal sealed class FusionCacheProvider(
    IFusionCache cache,
     IConnectionMultiplexer connectionMultiplexer,
    ILogger<FusionCacheProvider> logger)
    : ICacheProvider
{
    private static readonly Func<CancellationToken, Task<byte[]?>> EmptyFactory =
   static _ => Task.FromResult<byte[]?>(null!);

    private static readonly PlatformActivitySource ActivitySources = new();
    private static readonly CacheMeter Metrics = CacheMeter.Shared;
    public async Task<byte[]?> GetAsync(
    string key,
    CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        using var activity = ActivitySources.StartActivity(
            "FusionCacheProvider.GetAsync");

        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.operation", "get");
        Metrics.RecordLookup(CacheMetricConstants.FusionCacheProvider, CacheMetricConstants.GetOperation);

        try
        {
            var value = await cache.GetOrSetAsync<byte[]?>(
                key,
                EmptyFactory,
                token: cancellationToken);

            activity?.SetTag("cache.hit", value is not null);
            if (value is null)
            {
                Metrics.RecordMiss(CacheMetricConstants.FusionCacheProvider, CacheMetricConstants.GetOperation);
            }
            else
            {
                Metrics.RecordHit(CacheMetricConstants.FusionCacheProvider, CacheMetricConstants.GetOperation);
            }

            logger.LogDebug(
                "FusionCache {CacheResult} for key {CacheKey}",
                value is not null ? "hit" : "miss",
                key);

            return value;
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("cache.operation.cancelled", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

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
                "Error getting FusionCache entry for key {CacheKey}",
                key);

            throw;
        }
    }
    public async Task<byte[]> GetOrCreateAsync(
        string key,
        Func<CancellationToken, Task<byte[]>> factory,
        TimeSpan ttl,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        ValidateTtl(ttl);

        using var activity = ActivitySources.StartActivity(
            "FusionCacheProvider.GetOrCreateAsync",
            ActivityKind.Internal);

        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.ttl", ttl.ToString());
        activity?.SetTag("cache.operation", "get-or-create");
        Metrics.RecordLookup(CacheMetricConstants.FusionCacheProvider, CacheMetricConstants.GetOrCreateOperation);

        var factoryInvoked = false;

        try
        {
            var value = await cache.GetOrSetAsync(
                key,
                 async ct =>
                 {
                     factoryInvoked = true;
                     Metrics.RecordMiss(CacheMetricConstants.FusionCacheProvider, CacheMetricConstants.GetOrCreateOperation);
                     logger.LogDebug(
                         "Cache miss for key {CacheKey}. Executing factory.",
                         key);

                     return await factory(ct);
                 },
                duration: ttl,
                tags: tags,
                token: cancellationToken);

            activity?.SetTag("cache.operation.success", true);
            if (!factoryInvoked)
            {
                Metrics.RecordHit(CacheMetricConstants.FusionCacheProvider, CacheMetricConstants.GetOrCreateOperation);
            }

            logger.LogDebug(
                "FusionCache entry retrieved or created for key {CacheKey}",
                key);

            return value;
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("cache.operation.cancelled", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

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
                "Error getting or creating FusionCache entry for key {CacheKey}",
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

        ValidateTtl(ttl);

        using var activity = ActivitySources.StartActivity(
            "FusionCacheProvider.SetAsync",
            ActivityKind.Internal);

        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.ttl", ttl.ToString());
        activity?.SetTag("cache.operation", "set");

        try
        {
            await cache.SetAsync(
                key,
                value,
                duration: ttl,
                tags: tags,
                token: cancellationToken);

            activity?.SetTag("cache.operation.success", true);

            logger.LogDebug(
                "FusionCache entry set for key {CacheKey} with TTL {CacheTtl}",
                key,
                ttl);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("cache.operation.cancelled", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

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
                "Error setting FusionCache entry for key {CacheKey}",
                key);

            throw;
        }
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        using var activity = ActivitySources.StartActivity(
            "FusionCacheProvider.RemoveAsync",
            ActivityKind.Internal);

        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.operation", "remove");

        try
        {
            await cache.RemoveAsync(
                key,
                token: cancellationToken);

            activity?.SetTag("cache.operation.success", true);

            logger.LogDebug(
                "FusionCache entry removed for key {CacheKey}",
                key);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("cache.operation.cancelled", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

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
                "Error removing FusionCache entry for key {CacheKey}",
                key);

            throw;
        }
    }

    public async Task RemoveByTagAsync(
        string tag,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        using var activity = ActivitySources.StartActivity(
            "FusionCacheProvider.RemoveByTagAsync",
            ActivityKind.Internal);

        activity?.SetTag("cache.tag", tag);
        activity?.SetTag("cache.operation", "remove-by-tag");

        try
        {
            await cache.RemoveByTagAsync(
                tag,
                token: cancellationToken);

            activity?.SetTag("cache.operation.success", true);

            logger.LogDebug(
                "FusionCache entries removed by tag {CacheTag}",
                tag);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("cache.operation.cancelled", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

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
                "Error removing FusionCache entries by tag {CacheTag}",
                tag);

            throw;
        }
    }

    public async Task<bool> TryAddAsync(
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

        using var activity = ActivitySources.StartActivity(
            "FusionCacheProvider.TryAddAsync",
            ActivityKind.Internal);

        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.ttl", ttl.ToString());
        activity?.SetTag("cache.operation", "try-add");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var database = connectionMultiplexer.GetDatabase();

            var added = await database.StringSetAsync(
                key,
                value,
                ttl,
                when: When.NotExists);

            activity?.SetTag("cache.operation.success", true);
            activity?.SetTag("cache.added", added);
            if (added)
            {
            }

            logger.LogDebug(
                "Fusion cache {CacheResult} for key {CacheKey}",
                added ? "entry added" : "entry already exists",
                key);

            return added;
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("cache.operation.cancelled", true);

            logger.LogDebug(
                "Fusion cache try-add operation was cancelled for key {CacheKey}",
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
                "Error atomically adding Fusion cache entry for key {CacheKey}",
                key);

            throw;
        }
    }
    private static void ValidateTtl(TimeSpan ttl)
    {
        if (ttl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ttl),
                ttl,
                "Cache expiration must be greater than zero.");
        }
    }
}