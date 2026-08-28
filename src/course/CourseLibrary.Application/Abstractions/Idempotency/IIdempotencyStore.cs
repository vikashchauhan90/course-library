namespace CourseLibrary.Application.Abstractions.Idempotency;

public interface IIdempotencyStore
{
    Task<IdempotencyEntry> GetOrCreateAsync(
        string key,
        Func<CancellationToken, Task<IdempotencyEntry>> factory,
        TimeSpan ttl,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    Task StoreAsync(
        string key,
        IdempotencyEntry entry,
        TimeSpan ttl,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task RemoveByTagAsync(
        string tag,
        CancellationToken cancellationToken = default);
}