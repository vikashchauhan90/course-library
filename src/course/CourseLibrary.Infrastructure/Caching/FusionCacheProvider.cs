using CourseLibrary.Application.Abstractions.Caching;
using CourseLibrary.Infrastructure.Observability.Traces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using ZiggyCreatures.Caching.Fusion;

namespace CourseLibrary.Infrastructure.Caching;

internal sealed class FusionCacheProvider(
    IFusionCache cache,
    ILogger<FusionCacheProvider> logger)
    : ICacheProvider
{
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

        using var activity = ActivitySources.Infrastructure.StartActivity(
            "FusionCacheProvider.GetOrCreateAsync",
            ActivityKind.Internal);

        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.ttl", ttl.ToString());
        activity?.SetTag("cache.operation", "get-or-create");

        try
        {
            var value = await cache.GetOrSetAsync(
                key,
                 async ct =>
                 {
                     logger.LogDebug(
                         "Cache miss for key {CacheKey}. Executing factory.",
                         key);

                     return await factory(ct);
                 },
                duration: ttl,
                tags: tags,
                token: cancellationToken);

            activity?.SetTag("cache.operation.success", true);

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

        using var activity = ActivitySources.Infrastructure.StartActivity(
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

        using var activity = ActivitySources.Infrastructure.StartActivity(
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

        using var activity = ActivitySources.Infrastructure.StartActivity(
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