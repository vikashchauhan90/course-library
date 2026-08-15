namespace CourseLibrary.Infrastructure.Caching;

internal sealed class AsyncLock
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<IDisposable> LockAsync(
        CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        return new Releaser(_semaphore);
    }

    private sealed class Releaser(
        SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose()
        {
            semaphore.Release();
        }
    }
}