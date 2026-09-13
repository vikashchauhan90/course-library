namespace CourseLibrary.Application.Abstractions.Caching;

public interface ICacheProvider
{
    Task<byte[]?> GetAsync(
       string key,
       CancellationToken cancellationToken = default);
    Task<byte[]> GetOrCreateAsync(
        string key,
        Func<CancellationToken, Task<byte[]>> factory,
        TimeSpan ttl,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    Task SetAsync(
        string key,
        byte[] value,
        TimeSpan ttl,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task RemoveByTagAsync(
        string tag,
        CancellationToken cancellationToken = default);

    Task<bool> TryAddAsync(
        string key,
        byte[] value,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);
}
