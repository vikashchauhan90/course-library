namespace CourseLibrary.Infrastructure.Idempotency;

public interface IIdempotencyStore
{
    Task<object?> GetResponseAsync(string key, CancellationToken cancellationToken = default);
    Task StoreResponseAsync(string key, object response, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
