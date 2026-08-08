using Microsoft.Extensions.Caching.Memory;

namespace CourseLibrary.Infrastructure.Caching;

public sealed class MemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;

    public MemoryCacheProvider(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<object?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cache.TryGetValue(key, out var value) ? value : null);
    }

    public Task SetAsync(string key, object value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        _cache.Set(key, value, ttl);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cache.TryGetValue(key, out _));
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}
