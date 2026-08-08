using CourseLibrary.Infrastructure.Caching;

namespace CourseLibrary.Infrastructure.Idempotency;

public sealed class CacheIdempotencyStore : IIdempotencyStore
{
    private readonly ICacheProvider _cacheProvider;

    public CacheIdempotencyStore(ICacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider;
    }

    public Task<IdempotencyEntry?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        return _cacheProvider.GetAsync(key, cancellationToken)
            .ContinueWith(task => task.Result as IdempotencyEntry,
                cancellationToken,
                TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.Current);
    }

    public Task StoreAsync(string key, IdempotencyEntry entry, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        return _cacheProvider.SetAsync(key, entry, ttl, cancellationToken);
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
