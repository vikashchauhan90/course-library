using CourseLibrary.Application.Abstractions.Caching;
using CourseLibrary.Application.Abstractions.Idempotency;
using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Infrastructure.Observability.Traces;
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
            SerializerType.MessagePack);

    public async Task<IdempotencyEntry> GetOrCreateAsync(
        string key,
        Func<CancellationToken, Task<IdempotencyEntry>> factory,
        TimeSpan ttl,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        ValidateTtl(ttl);

        using var activity = ActivitySources.Infrastructure.StartActivity(
            "CacheIdempotencyStore.GetOrCreateAsync",
            ActivityKind.Internal);

        activity?.SetTag("idempotency.operation", "get-or-create");
        activity?.SetTag("idempotency.key", key);
        activity?.SetTag("idempotency.ttl", ttl.ToString());

        try
        {
            var data = await cacheProvider.GetOrCreateAsync(
                key,
                async ct =>
                {
                    var entry = await factory(ct);

                    ArgumentNullException.ThrowIfNull(entry);

                    return _serializer.Serialize(entry);
                },
                ttl,
                tags,
                cancellationToken);

            var result = _serializer.Deserialize(data);

            ArgumentNullException.ThrowIfNull(result);

            activity?.SetTag("idempotency.success", true);

            logger.LogDebug(
                "Idempotency entry retrieved or created for key {IdempotencyKey}",
                key);

            return result;
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("idempotency.cancelled", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

            logger.LogDebug(
                "Idempotency get-or-create operation was cancelled for key {IdempotencyKey}",
                key);

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag("idempotency.error", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                ex.Message);

            logger.LogError(
                ex,
                "Error getting or creating idempotency entry for key {IdempotencyKey}",
                key);

            throw;
        }
    }

    public async Task StoreAsync(
        string key,
        IdempotencyEntry entry,
        TimeSpan ttl,
        IEnumerable<string>? tags = null,
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

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var data = _serializer.Serialize(entry);

            await cacheProvider.SetAsync(
                key,
                data,
                ttl,
                tags,
                cancellationToken);

            activity?.SetTag("idempotency.success", true);

            logger.LogDebug(
                "Idempotency entry stored for key {IdempotencyKey}",
                key);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("idempotency.cancelled", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

            logger.LogDebug(
                "Idempotency store operation was cancelled for key {IdempotencyKey}",
                key);

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag("idempotency.error", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                ex.Message);

            logger.LogError(
                ex,
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
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

            logger.LogDebug(
                "Idempotency remove operation was cancelled for key {IdempotencyKey}",
                key);

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag("idempotency.error", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                ex.Message);

            logger.LogError(
                ex,
                "Error removing idempotency entry for key {IdempotencyKey}",
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
            "CacheIdempotencyStore.RemoveByTagAsync",
            ActivityKind.Internal);

        activity?.SetTag("idempotency.operation", "remove-by-tag");
        activity?.SetTag("idempotency.tag", tag);

        try
        {
            await cacheProvider.RemoveByTagAsync(
                tag,
                cancellationToken);

            activity?.SetTag("idempotency.success", true);

            logger.LogDebug(
                "Idempotency entries removed by tag {IdempotencyTag}",
                tag);
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("idempotency.cancelled", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Operation was cancelled.");

            logger.LogDebug(
                "Idempotency remove-by-tag operation was cancelled for tag {IdempotencyTag}",
                tag);

            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag("idempotency.error", true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                ex.Message);

            logger.LogError(
                ex,
                "Error removing idempotency entries for tag {IdempotencyTag}",
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
                "Idempotency TTL must be greater than zero.");
        }
    }
}