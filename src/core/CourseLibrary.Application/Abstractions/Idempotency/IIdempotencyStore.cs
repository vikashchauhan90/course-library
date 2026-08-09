namespace CourseLibrary.Application.Abstractions.Idempotency;

public interface IIdempotencyStore
{
    Task<IdempotencyEntry> GetOrCreateAsync(
        string key,
        Func<CancellationToken, Task<IdempotencyEntry>> factory,
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