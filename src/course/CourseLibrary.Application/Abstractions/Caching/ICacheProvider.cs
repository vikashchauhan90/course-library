namespace CourseLibrary.Application.Abstractions.Caching;

public interface ICacheProvider
{
    Task<byte[]> GetOrCreateAsync(
        string key,
        Func<CancellationToken, Task<byte[]>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);

    Task SetAsync(
        string key,
        byte[] value,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default);
}
