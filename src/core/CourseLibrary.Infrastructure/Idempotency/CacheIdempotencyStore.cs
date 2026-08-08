using CourseLibrary.Infrastructure.Caching;

namespace CourseLibrary.Infrastructure.Idempotency;

public sealed class CacheIdempotencyStore : IIdempotencyStore
{
    private readonly ICacheProvider _cacheProvider;

    public CacheIdempotencyStore(ICacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider;
    }

    public Task<object?> GetResponseAsync(string key, CancellationToken cancellationToken = default)
    {
        return _cacheProvider.GetAsync(key, cancellationToken);
    }

    public Task StoreResponseAsync(string key, object response, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        return _cacheProvider.SetAsync(key, response, ttl, cancellationToken);
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return _cacheProvider.ExistsAsync(key, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return _cacheProvider.RemoveAsync(key, cancellationToken);
    }
}
