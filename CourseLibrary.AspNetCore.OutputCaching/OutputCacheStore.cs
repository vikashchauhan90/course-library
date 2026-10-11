using CourseLibrary.Platform.Caching.Abstractions;
using CourseLibrary.Platform.Observability.Traces;
using CourseLibrary.AspNetCore.OutputCaching.Observability;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO.Hashing;
using System.Text;

namespace CourseLibrary.AspNetCore.OutputCaching;

public sealed class OutputCacheStore(
    ICacheProvider cacheProvider,
    ILogger<OutputCacheStore> logger)
    : IOutputCacheStore
{
    private static readonly PlatformActivitySource ActivitySources = new();
    public async ValueTask<byte[]?> GetAsync(
        string key,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var cacheKey = HashCacheKey(key);
        using var activity = ActivitySources.StartActivity(
            "OutputCacheStore.GetAsync");

        activity?.SetTag(OutputCacheActivityTagConstants.Operation, OutputCacheActivityTagConstants.GetOperation);

        try
        {
            var value = await cacheProvider.GetOrCreateAsync(
                cacheKey,
                factory: _ => Task.FromResult<byte[]>(Array.Empty<byte>()),
                TimeSpan.FromSeconds(1), // Temporary TTL for the factory, won't be used since we return null
                null,
                cancellationToken);

            var cacheHit = value is { Length: > 0 };
            activity?.SetTag(OutputCacheActivityTagConstants.Hit, cacheHit);
            activity?.SetTag(OutputCacheActivityTagConstants.OperationSuccess, true);

            logger.GetResult(cacheHit ? "hit" : "miss");

            return value?.Length > 0 ? value : null;
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag(OutputCacheActivityTagConstants.OperationCancelled, true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

            logger.GetCancelled();

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag(OutputCacheActivityTagConstants.OperationError, true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                ex.Message);

            logger.GetFailed(ex);

            throw;
        }
    }

    public async ValueTask SetAsync(
        string key,
        byte[] value,
        string[]? tags,
        TimeSpan validFor,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);
        var cacheKey = HashCacheKey(key);

        if (validFor <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(validFor),
                validFor,
                "Output cache expiration must be greater than zero.");
        }

        using var activity = ActivitySources.StartActivity(
            "OutputCacheStore.SetAsync");

        activity?.SetTag(OutputCacheActivityTagConstants.Ttl, validFor.ToString());
        activity?.SetTag(OutputCacheActivityTagConstants.Operation, OutputCacheActivityTagConstants.SetOperation);
        activity?.SetTag(OutputCacheActivityTagConstants.TagCount, tags?.Length ?? 0);

        try
        {
            await cacheProvider.SetAsync(
                cacheKey,
                value,
                validFor,
                tags,
                cancellationToken);

            activity?.SetTag(OutputCacheActivityTagConstants.OperationSuccess, true);

            logger.EntrySet(validFor);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag(OutputCacheActivityTagConstants.OperationCancelled, true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

            logger.SetCancelled();

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag(OutputCacheActivityTagConstants.OperationError, true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                ex.Message);

            logger.SetFailed(ex);

            throw;
        }
    }

    public async ValueTask EvictByTagAsync(
        string tag,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        using var activity = ActivitySources.StartActivity(
            "OutputCacheStore.EvictByTagAsync");

        activity?.SetTag(OutputCacheActivityTagConstants.Operation, OutputCacheActivityTagConstants.EvictByTagOperation);

        try
        {
            await cacheProvider.RemoveByTagAsync(
                tag,
                cancellationToken);

            activity?.SetTag(OutputCacheActivityTagConstants.OperationSuccess, true);

            logger.EntriesEvictedByTag();
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag(OutputCacheActivityTagConstants.OperationCancelled, true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

            logger.TagEvictionCancelled();

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag(OutputCacheActivityTagConstants.OperationError, true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                ex.Message);

            logger.TagEvictionFailed(ex);

            throw;
        }
    }

    private static string HashCacheKey(string key)
    {
        var hash = XxHash128.Hash(Encoding.UTF8.GetBytes(key));

        return Convert.ToHexString(hash);
    }
}