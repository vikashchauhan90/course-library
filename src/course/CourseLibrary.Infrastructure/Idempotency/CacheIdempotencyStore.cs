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
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        using var activity = ActivitySources.Infrastructure.StartActivity(
            "CacheIdempotencyStore.GetOrCreateAsync",
            ActivityKind.Internal
            );
        if (ttl <= TimeSpan.Zero)
        {
            activity?.AddTag("error", true)
                .AddTag("error.message", "Idempotency TTL must be greater than zero.");

            throw new ArgumentOutOfRangeException(
                nameof(ttl),
                ttl,
                "Idempotency TTL must be greater than zero.");
        }

        activity?.AddTag("idempotency.key", key)
            .AddTag("idempotency.ttl", ttl.ToString());
        activity?.AddTag("idempotency.factory", factory.Method.Name);
        var data = await cacheProvider.GetOrCreateAsync(
            key,
            async ct =>
            {
                var entry = await factory(ct);

                ArgumentNullException.ThrowIfNull(entry);

                return _serializer.Serialize(entry);
            },
            ttl,
            cancellationToken);

        var result = _serializer.Deserialize(data);

        logger.LogDebug(
            "Idempotency entry retrieved or created for key {IdempotencyKey}",
            key);

        return result;
    }

    public async Task StoreAsync(
        string key,
        IdempotencyEntry entry,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(entry);

        using var activity = ActivitySources.Infrastructure.StartActivity("CacheIdempotencyStore.StoreAsync", ActivityKind.Internal);

        if (ttl <= TimeSpan.Zero)
        {
            activity?.AddTag("error", true)
                .AddTag("error.message", "Idempotency TTL must be greater than zero.");
            throw new ArgumentOutOfRangeException(
                nameof(ttl),
                ttl,
                "Idempotency TTL must be greater than zero.");
        }

        var data = _serializer.Serialize(entry);

        activity?.AddTag("idempotency.key", key)
            .AddTag("idempotency.ttl", ttl.ToString())
            .AddTag("idempotency.entry", entry.ToString());

        await cacheProvider.SetAsync(
            key,
            data,
            ttl,
            cancellationToken);

        logger.LogDebug(
            "Idempotency entry stored for key {IdempotencyKey}",
            key);
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        using var activity = ActivitySources.Infrastructure.StartActivity("CacheIdempotencyStore.RemoveAsync", ActivityKind.Internal);
        activity?.AddTag("idempotency.key", key);
        activity?.AddTag("idempotency.operation", "remove");
        await cacheProvider.RemoveAsync(
            key,
            cancellationToken);

        logger.LogDebug(
            "Idempotency entry removed for key {IdempotencyKey}",
            key);
    }
}