namespace CourseLibrary.Platform.Idempotency.Abstractions;

public interface IIdempotencyStore
{
    Task<IdempotencyEntry?> GetAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task<bool> TryAcquireAsync(
        IdempotencyEntry entry,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);

    Task StoreAsync(
        string key,
        IdempotencyEntry entry,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default);
}