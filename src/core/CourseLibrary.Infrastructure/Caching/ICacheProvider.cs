namespace CourseLibrary.Infrastructure.Caching;

public interface ICacheProvider
{
    Task<object?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, object value, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
