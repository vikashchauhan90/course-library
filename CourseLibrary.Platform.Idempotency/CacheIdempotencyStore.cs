using CourseLibrary.Platform.Caching.Abstractions;
using CourseLibrary.Platform.Idempotency.Abstractions;
using CourseLibrary.Platform.Idempotency.Observability;
using CourseLibrary.Platform.Serialization.Abstractions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Platform.Idempotency;

internal sealed class CacheIdempotencyStore(
    ICacheProvider cacheProvider,
    ISerializerFactory serializerFactory,
    ILogger<CacheIdempotencyStore> logger)
    : IIdempotencyStore
{
    private readonly ISerializer<IdempotencyEntry> _serializer =
        serializerFactory.Create<IdempotencyEntry>(
            SerializerType.Json);

    private static readonly IdempotencyActivitySource ActivitySources = IdempotencyActivitySource.Shared;
    private static readonly IdempotencyMeter Metrics = IdempotencyMeter.Shared;

    public async Task<IdempotencyEntry?> GetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var startedAt = Stopwatch.GetTimestamp();

        using var activity = ActivitySources.StartActivity(
            "CacheIdempotencyStore.GetAsync");

        activity?.SetTag(IdempotencyActivityTagConstants.Operation, IdempotencyActivityTagConstants.GetOperation);

        try
        {
            var data = await cacheProvider.GetAsync(
                key,
                cancellationToken);

            if (data is null or { Length: 0 })
            {
                activity?.SetTag(IdempotencyActivityTagConstants.Found, false);
                RecordOutcome(activity, IdempotencyActivityTagConstants.GetOperation, "miss", startedAt);
                logger.LookupCompleted("miss");

                return null;
            }

            var entry = _serializer.Deserialize(data);

            ArgumentNullException.ThrowIfNull(entry);

            activity?.SetTag(IdempotencyActivityTagConstants.Found, true);
            activity?.SetTag(IdempotencyActivityTagConstants.Status, entry.Status.ToString());
            RecordOutcome(activity, IdempotencyActivityTagConstants.GetOperation, "hit", startedAt);
            logger.LookupCompleted("hit");

            return entry;
        }
        catch (OperationCanceledException)
        {
            RecordOutcome(activity, IdempotencyActivityTagConstants.GetOperation, "cancelled", startedAt);
            activity?.SetTag(IdempotencyActivityTagConstants.Cancelled, true);
            logger.OperationCancelled(IdempotencyActivityTagConstants.GetOperation);

            throw;
        }
        catch (Exception exception)
        {
            RecordOutcome(activity, IdempotencyActivityTagConstants.GetOperation, "error", startedAt);
            activity?.SetTag(IdempotencyActivityTagConstants.Error, true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Idempotency operation failed.");

            logger.OperationFailed(exception, IdempotencyActivityTagConstants.GetOperation);

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
        var startedAt = Stopwatch.GetTimestamp();

        using var activity = ActivitySources.StartActivity(
            "CacheIdempotencyStore.TryAcquireAsync",
            ActivityKind.Internal);

        activity?.SetTag(IdempotencyActivityTagConstants.Operation, IdempotencyActivityTagConstants.AcquireOperation);
        activity?.SetTag(IdempotencyActivityTagConstants.Ttl, ttl.ToString());

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var data = _serializer.Serialize(entry);

            var acquired = await cacheProvider.TryAddAsync(
                entry.Key,
                data,
                ttl,
                cancellationToken);

            activity?.SetTag(IdempotencyActivityTagConstants.Acquired, acquired);
            var outcome = acquired ? "acquired" : "already-acquired";
            RecordOutcome(activity, IdempotencyActivityTagConstants.AcquireOperation, outcome, startedAt);
            logger.AcquisitionCompleted(outcome);

            return acquired;
        }
        catch (OperationCanceledException)
        {
            RecordOutcome(activity, IdempotencyActivityTagConstants.AcquireOperation, "cancelled", startedAt);
            activity?.SetTag(IdempotencyActivityTagConstants.Cancelled, true);
            logger.OperationCancelled(IdempotencyActivityTagConstants.AcquireOperation);

            throw;
        }
        catch (Exception exception)
        {
            RecordOutcome(activity, IdempotencyActivityTagConstants.AcquireOperation, "error", startedAt);
            activity?.SetTag(IdempotencyActivityTagConstants.Error, true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Idempotency operation failed.");

            logger.OperationFailed(exception, IdempotencyActivityTagConstants.AcquireOperation);

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
        var startedAt = Stopwatch.GetTimestamp();

        using var activity = ActivitySources.StartActivity(
            "CacheIdempotencyStore.StoreAsync",
            ActivityKind.Internal);

        activity?.SetTag(IdempotencyActivityTagConstants.Operation, IdempotencyActivityTagConstants.StoreOperation);
        activity?.SetTag(IdempotencyActivityTagConstants.Ttl, ttl.ToString());
        activity?.SetTag(IdempotencyActivityTagConstants.Status, entry.Status.ToString());

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

            activity?.SetTag(IdempotencyActivityTagConstants.Outcome, "success");
            RecordOutcome(activity, IdempotencyActivityTagConstants.StoreOperation, "success", startedAt);
            logger.EntryStored(entry.Status.ToString());
        }
        catch (OperationCanceledException)
        {
            RecordOutcome(activity, IdempotencyActivityTagConstants.StoreOperation, "cancelled", startedAt);
            activity?.SetTag(IdempotencyActivityTagConstants.Cancelled, true);
            logger.OperationCancelled(IdempotencyActivityTagConstants.StoreOperation);

            throw;
        }
        catch (Exception exception)
        {
            RecordOutcome(activity, IdempotencyActivityTagConstants.StoreOperation, "error", startedAt);
            activity?.SetTag(IdempotencyActivityTagConstants.Error, true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Idempotency operation failed.");

            logger.OperationFailed(exception, IdempotencyActivityTagConstants.StoreOperation);

            throw;
        }
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var startedAt = Stopwatch.GetTimestamp();

        using var activity = ActivitySources.StartActivity(
            "CacheIdempotencyStore.RemoveAsync",
            ActivityKind.Internal);

        activity?.SetTag(IdempotencyActivityTagConstants.Operation, IdempotencyActivityTagConstants.RemoveOperation);

        try
        {
            await cacheProvider.RemoveAsync(
                key,
                cancellationToken);

            activity?.SetTag(IdempotencyActivityTagConstants.Outcome, "success");
            RecordOutcome(activity, IdempotencyActivityTagConstants.RemoveOperation, "success", startedAt);
            logger.EntryRemoved();
        }
        catch (OperationCanceledException)
        {
            RecordOutcome(activity, IdempotencyActivityTagConstants.RemoveOperation, "cancelled", startedAt);
            activity?.SetTag(IdempotencyActivityTagConstants.Cancelled, true);
            logger.OperationCancelled(IdempotencyActivityTagConstants.RemoveOperation);

            throw;
        }
        catch (Exception exception)
        {
            RecordOutcome(activity, IdempotencyActivityTagConstants.RemoveOperation, "error", startedAt);
            activity?.SetTag(IdempotencyActivityTagConstants.Error, true);
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Idempotency operation failed.");

            logger.OperationFailed(exception, IdempotencyActivityTagConstants.RemoveOperation);

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

    private static void RecordOutcome(
        Activity? activity,
        string operation,
        string outcome,
        long startedAt)
    {
        activity?.SetTag(IdempotencyActivityTagConstants.Outcome, outcome);
        Metrics.RecordOperation(
            operation,
            outcome,
            Stopwatch.GetElapsedTime(startedAt).TotalSeconds);
    }
}