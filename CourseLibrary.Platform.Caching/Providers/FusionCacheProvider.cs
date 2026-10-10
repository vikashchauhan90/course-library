
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

        activity?.SetTag(CacheActivityTagConstants.Key, key);
        activity?.SetTag(CacheActivityTagConstants.Operation, CacheActivityTagConstants.GetOperation);
        Metrics.RecordLookup(CacheActivityTagConstants.FusionCacheProvider, CacheActivityTagConstants.GetOperation);

        try
        {
            var value = await cache.GetOrSetAsync<byte[]?>(
                key,
                EmptyFactory,
                token: cancellationToken);

            activity?.SetTag(CacheActivityTagConstants.Hit, value is not null);
            if (value is null)
            {
                Metrics.RecordMiss(CacheActivityTagConstants.FusionCacheProvider, CacheActivityTagConstants.GetOperation);
            }
            else
            {
                Metrics.RecordHit(CacheActivityTagConstants.FusionCacheProvider, CacheActivityTagConstants.GetOperation);
            }

            logger.LogDebug(
                "FusionCache {CacheResult} for key {CacheKey}",
                value is not null ? "hit" : "miss",
                key);

            return value;
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag(CacheActivityTagConstants.OperationCancelled, true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag(CacheActivityTagConstants.OperationError, true);
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

        activity?.SetTag(CacheActivityTagConstants.Key, key);
        activity?.SetTag(CacheActivityTagConstants.Ttl, ttl.ToString());
        activity?.SetTag(CacheActivityTagConstants.Operation, CacheActivityTagConstants.GetOrCreateOperation);
        Metrics.RecordLookup(CacheActivityTagConstants.FusionCacheProvider, CacheActivityTagConstants.GetOrCreateOperation);

        var factoryInvoked = false;

        try
        {
            var value = await cache.GetOrSetAsync(
                key,
                 async ct =>
                 {
                     factoryInvoked = true;
                     Metrics.RecordMiss(CacheActivityTagConstants.FusionCacheProvider, CacheActivityTagConstants.GetOrCreateOperation);
                     logger.LogDebug(
                         "Cache miss for key {CacheKey}. Executing factory.",
                         key);

                     return await factory(ct);
                 },
                duration: ttl,
                tags: tags,
                token: cancellationToken);

            activity?.SetTag(CacheActivityTagConstants.OperationSuccess, true);
            if (!factoryInvoked)
            {
                Metrics.RecordHit(CacheActivityTagConstants.FusionCacheProvider, CacheActivityTagConstants.GetOrCreateOperation);
            }

            logger.LogDebug(
                "FusionCache entry retrieved or created for key {CacheKey}",
                key);

            return value;
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag(CacheActivityTagConstants.OperationCancelled, true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag(CacheActivityTagConstants.OperationError, true);
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

        activity?.SetTag(CacheActivityTagConstants.Key, key);
        activity?.SetTag(CacheActivityTagConstants.Ttl, ttl.ToString());
        activity?.SetTag(CacheActivityTagConstants.Operation, CacheActivityTagConstants.SetOperation);

        try
        {
            await cache.SetAsync(
                key,
                value,
                duration: ttl,
                tags: tags,
                token: cancellationToken);

            activity?.SetTag(CacheActivityTagConstants.OperationSuccess, true);

            logger.LogDebug(
                "FusionCache entry set for key {CacheKey} with TTL {CacheTtl}",
                key,
                ttl);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag(CacheActivityTagConstants.OperationCancelled, true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag(CacheActivityTagConstants.OperationError, true);
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

        activity?.SetTag(CacheActivityTagConstants.Key, key);
        activity?.SetTag(CacheActivityTagConstants.Operation, CacheActivityTagConstants.RemoveOperation);

        try
        {
            await cache.RemoveAsync(
                key,
                token: cancellationToken);

            activity?.SetTag(CacheActivityTagConstants.OperationSuccess, true);

            logger.LogDebug(
                "FusionCache entry removed for key {CacheKey}",
                key);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag(CacheActivityTagConstants.OperationCancelled, true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag(CacheActivityTagConstants.OperationError, true);
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

        activity?.SetTag(CacheActivityTagConstants.Tag, tag);
        activity?.SetTag(CacheActivityTagConstants.Operation, CacheActivityTagConstants.RemoveByTagOperation);

        try
        {
            await cache.RemoveByTagAsync(
                tag,
                token: cancellationToken);

            activity?.SetTag(CacheActivityTagConstants.OperationSuccess, true);

            logger.LogDebug(
                "FusionCache entries removed by tag {CacheTag}",
                tag);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag(CacheActivityTagConstants.OperationCancelled, true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag(CacheActivityTagConstants.OperationError, true);
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

        activity?.SetTag(CacheActivityTagConstants.Key, key);
        activity?.SetTag(CacheActivityTagConstants.Ttl, ttl.ToString());
        activity?.SetTag(CacheActivityTagConstants.Operation, CacheActivityTagConstants.TryAddOperation);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var database = connectionMultiplexer.GetDatabase();

            var added = await database.StringSetAsync(
                key,
                value,
                ttl,
                when: When.NotExists);

            activity?.SetTag(CacheActivityTagConstants.OperationSuccess, true);
            activity?.SetTag(CacheActivityTagConstants.Added, added);
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
            activity?.SetTag(CacheActivityTagConstants.OperationCancelled, true);

            logger.LogDebug(
                "Fusion cache try-add operation was cancelled for key {CacheKey}",
                key);

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag(CacheActivityTagConstants.OperationError, true);
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