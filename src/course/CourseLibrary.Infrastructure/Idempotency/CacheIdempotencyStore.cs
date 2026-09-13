using CourseLibrary.Application.Abstractions.Caching;
using CourseLibrary.Application.Abstractions.Idempotency;
using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Infrastructure.Observability.Traces;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Infrastructure.Idempotency;

public sealed class CacheIdempotencyStore(
    ICacheProvider cacheProvider,
    ISerializerFactory serializerFactory,
    ILogger<CacheIdempotencyStore> logger)
    : IIdempotencyStore
{
    private readonly ISerializer<IdempotencyEntry> _serializer =
        serializerFactory.Create<IdempotencyEntry>(
            SerializerType.Json);

    public async Task<IdempotencyEntry?> GetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        using var activity = ActivitySources.Infrastructure.StartActivity(
            "CacheIdempotencyStore.GetAsync",
            ActivityKind.Internal);

        activity?.SetTag("idempotency.operation", "get");
        activity?.SetTag("idempotency.key", key);

        try
        {
            var data = await cacheProvider.GetAsync(
                key,
                cancellationToken);

            if (data is null or { Length: 0 })
            {
                activity?.SetTag("idempotency.found", false);

                logger.LogDebug(
                    "No idempotency entry found for key {IdempotencyKey}",
                    key);

                return null;
            }

            var entry = _serializer.Deserialize(data);

            ArgumentNullException.ThrowIfNull(entry);

            activity?.SetTag("idempotency.found", true);
            activity?.SetTag(
                "idempotency.status",
                entry.Status.ToString());

            return entry;
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("idempotency.cancelled", true);

            logger.LogDebug(
                "Idempotency get operation was cancelled for key {IdempotencyKey}",
                key);

            throw;
        }
        catch (Exception exception)
        {
            activity?.SetTag("idempotency.error", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                exception.Message);

            logger.LogError(
                exception,
                "Error getting idempotency entry for key {IdempotencyKey}",
                key);

            throw;
        }
    }

    public async Task<bool> TryAcquireAsync(
        IdempotencyEntry entry,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        ArgumentException.ThrowIfNullOrWhiteSpace(entry.Key);

        ValidateTtl(ttl);

        using var activity = ActivitySources.Infrastructure.StartActivity(
            "CacheIdempotencyStore.TryAcquireAsync",
            ActivityKind.Internal);

        activity?.SetTag("idempotency.operation", "try-acquire");
        activity?.SetTag("idempotency.key", entry.Key);
        activity?.SetTag("idempotency.ttl", ttl.ToString());

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var data = _serializer.Serialize(entry);

            var acquired = await cacheProvider.TryAddAsync(
                entry.Key,
                data,
                ttl,
                cancellationToken);

            activity?.SetTag(
                "idempotency.acquired",
                acquired);

            if (acquired)
            {
                logger.LogDebug(
                    "Idempotency key {IdempotencyKey} acquired",
                    entry.Key);
            }
            else
            {
                logger.LogDebug(
                    "Idempotency key {IdempotencyKey} was already acquired",
                    entry.Key);
            }

            return acquired;
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("idempotency.cancelled", true);

            logger.LogDebug(
                "Idempotency acquire operation was cancelled for key {IdempotencyKey}",
                entry.Key);

            throw;
        }
        catch (Exception exception)
        {
            activity?.SetTag("idempotency.error", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                exception.Message);

            logger.LogError(
                exception,
                "Error acquiring idempotency key {IdempotencyKey}",
                entry.Key);

            throw;
        }
    }

    public async Task StoreAsync(
        string key,
        IdempotencyEntry entry,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(entry);

        ValidateTtl(ttl);

        using var activity = ActivitySources.Infrastructure.StartActivity(
            "CacheIdempotencyStore.StoreAsync",
            ActivityKind.Internal);

        activity?.SetTag("idempotency.operation", "store");
        activity?.SetTag("idempotency.key", key);
        activity?.SetTag("idempotency.ttl", ttl.ToString());
        activity?.SetTag(
            "idempotency.status",
            entry.Status.ToString());

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var data = _serializer.Serialize(entry);

            var tags = new[]
            {
                $"idempotency:{key}"
            };

            await cacheProvider.SetAsync(
                key,
                data,
                ttl,
                tags,
                cancellationToken);

            activity?.SetTag("idempotency.success", true);

            logger.LogDebug(
                "Idempotency entry stored for key {IdempotencyKey} with status {IdempotencyStatus}",
                key,
                entry.Status);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("idempotency.cancelled", true);

            logger.LogDebug(
                "Idempotency store operation was cancelled for key {IdempotencyKey}",
                key);

            throw;
        }
        catch (Exception exception)
        {
            activity?.SetTag("idempotency.error", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                exception.Message);

            logger.LogError(
                exception,
                "Error storing idempotency entry for key {IdempotencyKey}",
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
            "CacheIdempotencyStore.RemoveAsync",
            ActivityKind.Internal);

        activity?.SetTag("idempotency.operation", "remove");
        activity?.SetTag("idempotency.key", key);

        try
        {
            await cacheProvider.RemoveAsync(
                key,
                cancellationToken);

            activity?.SetTag("idempotency.success", true);

            logger.LogDebug(
                "Idempotency entry removed for key {IdempotencyKey}",
                key);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("idempotency.cancelled", true);

            logger.LogDebug(
                "Idempotency remove operation was cancelled for key {IdempotencyKey}",
                key);

            throw;
        }
        catch (Exception exception)
        {
            activity?.SetTag("idempotency.error", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                exception.Message);

            logger.LogError(
                exception,
                "Error removing idempotency entry for key {IdempotencyKey}",
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
                "Idempotency TTL must be greater than zero.");
        }
    }
}