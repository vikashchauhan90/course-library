using CourseLibrary.Idp.Application.Abstractions.Caching;
using CourseLibrary.Idp.Application.Abstractions.Idempotency;
using CourseLibrary.Idp.Application.Abstractions.Serialization;
using CourseLibrary.Idp.Application.Abstractions.Serializers;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Idp.Infrastructure.Idempotency;

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

        if (ttl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ttl),
                ttl,
                "Idempotency TTL must be greater than zero.");
        }

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

        if (ttl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ttl),
                ttl,
                "Idempotency TTL must be greater than zero.");
        }

        var data = _serializer.Serialize(entry);

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

        await cacheProvider.RemoveAsync(
            key,
            cancellationToken);

        logger.LogDebug(
            "Idempotency entry removed for key {IdempotencyKey}",
            key);
    }
}