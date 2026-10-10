
using CourseLibrary.Platform.Caching.Abstractions;
using CourseLibrary.Platform.Caching.Observability;
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

            logger.LookupResult(value is not null ? "hit" : "miss");

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

            logger.OperationFailed(ex, CacheActivityTagConstants.GetOperation);

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
            "FusionCacheProvider.GetOrCreateAsync");

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
                     logger.MissExecutingFactory();

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

            logger.GetOrCreateCompleted();

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

            logger.OperationFailed(ex, CacheActivityTagConstants.GetOrCreateOperation);

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
            "FusionCacheProvider.SetAsync");

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

            logger.EntrySet();
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

            logger.OperationFailed(ex, CacheActivityTagConstants.SetOperation);

            throw;
        }
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        using var activity = ActivitySources.StartActivity(
            "FusionCacheProvider.RemoveAsync");

        activity?.SetTag(CacheActivityTagConstants.Operation, CacheActivityTagConstants.RemoveOperation);

        try
        {
            await cache.RemoveAsync(
                key,
                token: cancellationToken);

            activity?.SetTag(CacheActivityTagConstants.OperationSuccess, true);

            logger.EntryRemoved();
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

            logger.OperationFailed(ex, CacheActivityTagConstants.RemoveOperation);

            throw;
        }
    }

    public async Task RemoveByTagAsync(
        string tag,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        using var activity = ActivitySources.StartActivity(
            "FusionCacheProvider.RemoveByTagAsync");

        activity?.SetTag(CacheActivityTagConstants.Operation, CacheActivityTagConstants.RemoveByTagOperation);

        try
        {
            await cache.RemoveByTagAsync(
                tag,
                token: cancellationToken);

            activity?.SetTag(CacheActivityTagConstants.OperationSuccess, true);

            logger.EntriesRemovedByTag();
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

            logger.OperationFailed(ex, CacheActivityTagConstants.RemoveByTagOperation);

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
            "FusionCacheProvider.TryAddAsync");

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

            logger.TryAddResult(added ? "added" : "already exists");

            return added;
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag(CacheActivityTagConstants.OperationCancelled, true);

            logger.TryAddCancelled();

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag(CacheActivityTagConstants.OperationError, true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                ex.Message);

            logger.OperationFailed(ex, CacheActivityTagConstants.TryAddOperation);

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